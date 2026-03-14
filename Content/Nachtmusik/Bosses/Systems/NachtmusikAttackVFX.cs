using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;
using static MagnumOpus.Content.Nachtmusik.Bosses.Systems.NachtmusikSkySystem;

namespace MagnumOpus.Content.Nachtmusik.Bosses.Systems
{
    /// <summary>
    /// Nachtmusik boss attack choreography — Queen of Radiance.
    /// 4-phase system: Evening Star → Cosmic Dance → Celestial Crescendo → Supernova.
    /// Every attack telegraph → release → impact has phase-appropriate visual language:
    ///   P1: Crescent-moon projectiles, silver sparkle, gentle constellation patterns
    ///   P2: Arpeggio spirals, chord bursts, nebula-cloud pulsing, cosmic blue/indigo
    ///   P3: Starlight beams refracting into prismatic trails, constellation laser-line telegraphs
    ///   P4: Overwhelming supernova radiance, blinding stellar light, aurora afterimages
    /// </summary>
    public static class NachtmusikAttackVFX
    {
        // Nachtmusik palette — NO gold. Deep indigo, starlight silver, cosmic blue, nebula purple, white radiance.
        private static readonly Color DeepIndigo = new Color(25, 20, 65);
        private static readonly Color StarlightSilver = new Color(200, 215, 240);
        private static readonly Color CosmicBlue = new Color(60, 100, 190);
        private static readonly Color NebulaPurple = new Color(70, 40, 120);
        private static readonly Color WhiteRadiance = new Color(245, 245, 255);

        #region Phase 1 — Evening Star (Planetarium)

        /// <summary>StarlightWaltz: Graceful silver arcs trace constellation outlines.</summary>
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
            TriggerStarlightFlash(4f);
        }

        /// <summary>ConstellationDance: Points of light forming ephemeral star patterns.</summary>
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
            TriggerStarlightFlash(6f);
            var bloom = new BloomParticle(position, Vector2.Zero, StarlightSilver * 0.5f, 0.5f, 18);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        /// <summary>MoonbeamCascade: Falling streams of pale silver moonlight beams.</summary>
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

        /// <summary>NocturnalSerenade: Gentle sound wave rings expanding outward.</summary>
        public static void NocturnalSerenadeTelegraph(Vector2 center)
        {
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { DeepIndigo, StarlightSilver, CosmicBlue }, 0.4f);
        }

        public static void NocturnalSerenadeRelease(Vector2 center, int waveIndex)
        {
            CustomParticles.HaloRing(center, Color.Lerp(StarlightSilver, CosmicBlue, waveIndex / 5f),
                0.4f + waveIndex * 0.1f, 18);
            Phase10BossVFX.DynamicsWave(center, 0.4f + waveIndex * 0.1f, StarlightSilver);
            TriggerStarlightFlash(5f);
        }

        /// <summary>CrescentSlash: Crescent-moon shaped projectile arcs with silver sparkle trails.</summary>
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
            TriggerStarlightFlash(7f);
            var bloom = new BloomParticle(position, Vector2.Zero, CosmicBlue * 0.5f, 0.5f, 18);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        #endregion

        #region Phase 1B — Rising Tension (late Evening Star)

        /// <summary>AuroraVeil: Shimmering curtain of silver-blue light.</summary>
        public static void AuroraVeilTelegraph(Vector2 start, Vector2 end)
        {
            TelegraphSystem.LaserPath(start, end, 40f, 30, CosmicBlue * 0.4f);
            Phase10BossVFX.LegatoWaveWash(start, (end - start).SafeNormalize(Vector2.UnitX), CosmicBlue, (end - start).Length());
        }

        public static void AuroraVeilShimmer(Vector2 position, int frame)
        {
            // Silver-blue shimmer — no gold
            Color color = Color.Lerp(CosmicBlue, StarlightSilver, (float)Math.Sin(frame * 0.1f) * 0.5f + 0.5f);
            CustomParticles.GenericFlare(position, color, 0.3f, 10);
        }

        /// <summary>CosmicTempest: Swirling vortex of stellar energy — transition to Phase 2.</summary>
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
            TriggerCosmicFlash(6f + pulseIndex * 1.5f);
        }

        #endregion

        #region Phase 2 — Cosmic Dance (Orbiting Constellations, Nebula Pulses)

        /// <summary>NebulaBurst: Deep indigo/cosmic blue nebula cloud detonation.</summary>
        public static void NebulaBurstTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 200f, 35, NebulaPurple * 0.7f);
            Phase10BossVFX.FortissimoFlashWarning(center, CosmicBlue, 1.0f);
        }

        public static void NebulaBurstImpact(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(15f);
            CustomParticles.GenericFlare(center, CosmicBlue, 1.5f, 22);
            CustomParticles.ExplosionBurst(center, NebulaPurple, 12, 6f);
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Color color = i % 2 == 0 ? CosmicBlue : NebulaPurple;
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 50f, color, 0.5f, 16);
            }
            TriggerCosmicFlash(12f);
            var bloom = new BloomParticle(center, Vector2.Zero, CosmicBlue * 0.6f, 0.6f, 20);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        /// <summary>ArpeggioSpiralAttack: Musical arpeggio spirals — notes cascade in pitch order.</summary>
        public static void ArpeggioSpiralTelegraph(Vector2 center)
        {
            Phase10BossVFX.ArpeggioCascade(center, CosmicBlue, 8, 64f);
            TelegraphSystem.ConvergingRing(center, 160f, 35, StarlightSilver * 0.5f);
        }

        public static void ArpeggioSpiralRelease(Vector2 center, int noteIndex, Vector2 direction)
        {
            // Each note in the arpeggio gets progressively brighter
            float t = noteIndex / 8f;
            Color color = Color.Lerp(CosmicBlue, StarlightSilver, t);
            CustomParticles.GenericFlare(center + direction * (20f + noteIndex * 15f), color, 0.3f + t * 0.3f, 12);
            BossVFXOptimizer.ProjectileTrail(center, direction * 4f, color);
            if (noteIndex == 0) TriggerCosmicFlash(6f);
        }

        /// <summary>ChordBurstAttack: Massive chord — multiple notes fire simultaneously in all directions.</summary>
        public static void ChordBurstTelegraph(Vector2 center)
        {
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { DeepIndigo, CosmicBlue, NebulaPurple, StarlightSilver }, 1.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, StarlightSilver, 1.2f);
        }

        public static void ChordBurstRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(12f);
            CustomParticles.GenericFlare(center, StarlightSilver, 1.5f, 22);
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Color color = i % 3 == 0 ? NebulaPurple : (i % 3 == 1 ? CosmicBlue : StarlightSilver);
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 60f, color, 0.5f, 16);
            }
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { CosmicBlue, NebulaPurple, StarlightSilver }, 1.2f);
            TriggerCosmicFlash(14f);
            // Bloom ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                Color bColor = Color.Lerp(CosmicBlue, NebulaPurple, i / 8f);
                var bloom = new BloomParticle(center, vel, bColor * 0.5f, 0.5f, 20);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
        }

        /// <summary>EternalNightmare: Void tendrils of deep indigo reaching outward.</summary>
        public static void EternalNightmareTelegraph(Vector2 center)
        {
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { DeepIndigo, CosmicBlue }, 0.8f);
            Phase10BossVFX.DissonanceStorm(center, 150f, DeepIndigo, NebulaPurple);
        }

        public static void EternalNightmareRelease(Vector2 center, int tendrilIndex, Vector2 direction)
        {
            Vector2 pos = center + direction * (30f + tendrilIndex * 20f);
            Color color = Color.Lerp(DeepIndigo, NebulaPurple, tendrilIndex * 0.15f);
            CustomParticles.GenericFlare(pos, color, 0.4f + tendrilIndex * 0.05f, 16);
            BossVFXOptimizer.ProjectileTrail(pos, direction * 3f, color);
            if (tendrilIndex == 0)
                TriggerCosmicFlash(8f);
        }

        #endregion

        #region Phase 3 — Celestial Crescendo (Galaxy, Prismatic Beams, Constellation Lasers)

        /// <summary>GalacticJudgment: Massive radial constellation laser pattern.</summary>
        public static void GalacticJudgmentTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 250f, 50, CosmicBlue * 0.7f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { DeepIndigo, CosmicBlue, NebulaPurple, StarlightSilver }, 1.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, WhiteRadiance, 1.2f);
        }

        public static void GalacticJudgmentRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(20f);
            CustomParticles.GenericFlare(center, WhiteRadiance, 1.8f, 25);
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Color color = i % 3 == 0 ? NebulaPurple : (i % 3 == 1 ? CosmicBlue : StarlightSilver);
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 70f, color, 0.5f, 18);
            }
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { CosmicBlue, NebulaPurple, WhiteRadiance }, 1.5f);
            TriggerPrismaticFlash(15f);
            // Prismatic bloom ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                Color bColor = Color.Lerp(CosmicBlue, WhiteRadiance, i / 8f);
                var bloom = new BloomParticle(center, vel, bColor * 0.5f, 0.5f, 20);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
        }

        /// <summary>StarfallApocalypse: Massive comet-trail projectile shower from the rotating galaxy.</summary>
        public static void StarfallApocalypseTelegraph(Vector2 targetArea)
        {
            TelegraphSystem.DangerZone(targetArea, 350f, 50, CosmicBlue * 0.5f);
            Phase10BossVFX.CrescendoDangerRings(targetArea, NebulaPurple, 1.2f);
        }

        public static void StarfallApocalypseImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(12f);
            CustomParticles.ExplosionBurst(position, NebulaPurple, 10, 5f);
            CustomParticles.HaloRing(position, CosmicBlue, 0.6f, 18);
            CustomParticles.GenericFlare(position, StarlightSilver, 0.8f, 15);
            TriggerPrismaticFlash(10f);
            var bloom = new BloomParticle(position, Vector2.Zero, CosmicBlue * 0.5f, 0.5f, 18);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        /// <summary>ConstellationLaserTelegraph: Constellation lines become laser-path danger zones.</summary>
        public static void ConstellationLaserTelegraph(Vector2 start, Vector2 end)
        {
            TelegraphSystem.LaserPath(start, end, 50f, 40, StarlightSilver * 0.7f);
            // Starlight dots along the path
            Vector2 dir = (end - start);
            float length = dir.Length();
            dir.Normalize();
            for (int i = 0; i < 6; i++)
            {
                Vector2 pos = start + dir * (length * i / 6f);
                CustomParticles.GenericFlare(pos, StarlightSilver * 0.5f, 0.25f, 20);
            }
        }

        public static void ConstellationLaserFire(Vector2 start, Vector2 end)
        {
            MagnumScreenEffects.AddScreenShake(10f);
            Vector2 mid = (start + end) * 0.5f;
            CustomParticles.GenericFlare(mid, WhiteRadiance, 1.0f, 15);
            CustomParticles.HaloRing(start, CosmicBlue, 0.5f, 12);
            CustomParticles.HaloRing(end, CosmicBlue, 0.5f, 12);
            TriggerPrismaticFlash(8f);
        }

        /// <summary>PrismaticBeamRefraction: Starlight beams that refract into prismatic trails.</summary>
        public static void PrismaticBeamTelegraph(Vector2 origin, Vector2 direction)
        {
            TelegraphSystem.ThreatLine(origin, direction, 600f, 35, WhiteRadiance * 0.5f);
            Phase10BossVFX.StaffLineConvergence(origin, WhiteRadiance, 0.9f);
        }

        public static void PrismaticBeamImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(9f);
            // Prismatic split — silver core refracting into blue and purple
            CustomParticles.GenericFlare(position, WhiteRadiance, 0.8f, 14);
            CustomParticles.HaloRing(position, StarlightSilver, 0.6f, 16);
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Color refracted = i % 2 == 0 ? CosmicBlue : NebulaPurple;
                Vector2 vel = angle.ToRotationVector2() * 2.5f;
                var bloom = new BloomParticle(position, vel, refracted * 0.5f, 0.4f, 16);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
            TriggerPrismaticFlash(9f);
        }

        /// <summary>CelestialCharge: Blazing multi-dash through galaxy arms with prismatic afterimages.</summary>
        public static void CelestialChargeTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            TelegraphSystem.ThreatLine(position, dir, 700f, 30, CosmicBlue * 0.8f);
            Phase10BossVFX.FortissimoFlashWarning(position, WhiteRadiance, 1.0f);
        }

        public static void CelestialChargeAfterimage(Vector2 position, int dashNumber)
        {
            float intensity = 0.4f + dashNumber * 0.15f;
            Color trailColor = Color.Lerp(CosmicBlue, WhiteRadiance, dashNumber / 6f);
            CustomParticles.GenericFlare(position, trailColor, intensity, 12);
            BossVFXOptimizer.OptimizedHalo(position, trailColor, intensity, 10);
            TriggerPrismaticFlash(5f + dashNumber * 2f);
        }

        #endregion

        #region Phase 4 — Supernova (Blinding Stellar Radiance)

        /// <summary>SupernovaCollapse: The Queen becomes a miniature sun — blinding stellar explosion.</summary>
        public static void SupernovaCollapseTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 300f, 60, WhiteRadiance);
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { CosmicBlue, NebulaPurple, WhiteRadiance }, 1.5f);
        }

        public static void SupernovaCollapseRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(25f);
            CustomParticles.GenericFlare(center, WhiteRadiance, 2.5f, 30);
            CustomParticles.ExplosionBurst(center, CosmicBlue, 20, 8f);
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Color color = Color.Lerp(CosmicBlue, WhiteRadiance, i / 16f);
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 90f, color, 0.6f, 20);
            }
            Phase10BossVFX.CodaFinale(center, WhiteRadiance, DeepIndigo, 2.0f);
            TriggerSupernovaFlash(20f);
            // 16-point supernova bloom ring — silver/blue/white
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * 5f;
                Color bColor = Color.Lerp(CosmicBlue, WhiteRadiance, i / 16f);
                var bloom = new BloomParticle(center, vel, bColor * 0.6f, 0.6f, 25);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
        }

        /// <summary>StellarOverload: Continuous overwhelming light — every impact radiates aurora.</summary>
        public static void StellarOverloadImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(14f);
            CustomParticles.GenericFlare(position, WhiteRadiance, 1.2f, 20);
            CustomParticles.ExplosionBurst(position, StarlightSilver, 8, 5f);
            CustomParticles.HaloRing(position, CosmicBlue, 0.7f, 16);
            // Aurora afterimage sparkles
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 2f;
                Color auroraColor = Color.Lerp(CosmicBlue, NebulaPurple, i / 6f);
                var sparkle = new SparkleParticle(position + vel * 10f, vel, auroraColor * 0.6f, 0.4f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            TriggerSupernovaFlash(12f);
        }

        #endregion

        #region Special Attacks (Phase-agnostic)

        /// <summary>TwilightReversal: Time-reversal with inverted color palette — night becomes day.</summary>
        public static void TwilightReversalTelegraph(Vector2 center)
        {
            Color invertedIndigo = new Color(230, 235, 190);
            TelegraphSystem.ConvergingRing(center, 200f, 40, invertedIndigo * 0.6f);
            Phase10BossVFX.TempoShiftDistortion(center, 120f, 60f, 100f);
        }

        public static void TwilightReversalRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(18f);
            Color invertedBlue = new Color(195, 155, 65);
            Color invertedPurple = new Color(185, 215, 135);
            CustomParticles.GenericFlare(center, invertedBlue, 1.5f, 20);
            CustomParticles.GenericFlare(center, invertedPurple, 1.2f, 18);
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 60f, invertedBlue, 0.4f, 15);
            }
            Phase10BossVFX.KeyChangeFlash(center, CosmicBlue, invertedBlue, 1f);
            TriggerCosmicFlash(14f);
            var bloom = new BloomParticle(center, Vector2.Zero, invertedPurple * 0.5f, 0.6f, 20);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        /// <summary>QuantumBlink: Rapid multi-teleport with silver flash trails between positions.</summary>
        public static void QuantumBlinkFlash(Vector2 fromPos, Vector2 toPos)
        {
            CustomParticles.GenericFlare(fromPos, StarlightSilver, 0.8f, 6);
            CustomParticles.HaloRing(fromPos, CosmicBlue * 0.6f, 0.3f, 8);

            CustomParticles.GenericFlare(toPos, WhiteRadiance, 0.9f, 8);
            CustomParticles.HaloRing(toPos, CosmicBlue * 0.7f, 0.4f, 10);

            Vector2 dir = (toPos - fromPos);
            float length = dir.Length();
            dir.Normalize();
            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = fromPos + dir * (length * i / 5f);
                CustomParticles.GenericFlare(pos, StarlightSilver * 0.4f, 0.2f, 5);
            }
            TriggerStarlightFlash(5f);
        }

        public static void QuantumBlinkAttack(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(8f);
            CustomParticles.ExplosionBurst(position, CosmicBlue, 8, 4f);
            Phase10BossVFX.StaccatoMultiBurst(position, NebulaPurple, 3, 40f);
            TriggerPrismaticFlash(8f);
            var bloom = new BloomParticle(position, Vector2.Zero, CosmicBlue * 0.5f, 0.5f, 18);
            MagnumParticleHandler.SpawnParticle(bloom);
        }

        #endregion
    }
}