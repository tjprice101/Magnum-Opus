using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Autumn.Bosses.Systems
{
    /// <summary>
    /// Autunno boss attack choreography system.
    /// Manages attack-specific VFX sequences, telegraphs,
    /// and multi-layered impact effects for each attack pattern.
    /// Theme: Decay, withering, falling leaves, harvest twilight.
    /// </summary>
    public static class AutunnoAttackVFX
    {
        private static readonly Color AutumnOrange = new Color(200, 120, 40);
        private static readonly Color DecayBrown = new Color(100, 60, 30);
        private static readonly Color HarvestGold = new Color(180, 160, 60);
        private static readonly Color WitheredRed = new Color(150, 50, 30);

        #region Core Attack VFX

        /// <summary>LeafStorm: Swirling vortex of decaying leaves with amber sparks.</summary>
        public static void LeafStormTelegraph(Vector2 position, Vector2 direction)
        {
            TelegraphSystem.ThreatLine(position, direction, 350f, 30, AutumnOrange * 0.7f);
            Phase10BossVFX.AccelerandoSpiral(position, HarvestGold, 0.6f);
            BossVFXOptimizer.WarningFlare(position, 0.5f);
        }

        public static void LeafStormImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(8f);
            CustomParticles.ExplosionBurst(position, AutumnOrange, 10);
            Phase10BossVFX.CymbalCrashBurst(position, 0.7f);
            CustomParticles.HaloRing(position, DecayBrown, 0.5f, 15);
            CustomParticles.GenericMusicNotes(position, AutumnOrange, 4, 50f);
        }

        /// <summary>WitheringWind: Gusts of decay energy sweeping across the arena.</summary>
        public static void WitheringWindTelegraph(Vector2 position, Vector2 direction)
        {
            TelegraphSystem.ThreatLine(position, direction, 500f, 30, DecayBrown * 0.6f);
            Phase10BossVFX.GlissandoSlideWarning(position, position + direction * 500f, AutumnOrange, 0.5f);
        }

        public static void WitheringWindTrail(Vector2 position, Vector2 velocity)
        {
            CustomParticles.GenericFlare(position, DecayBrown, 0.3f, 8);
            CustomParticles.GlowTrail(position, AutumnOrange, 0.25f);
            if (Main.rand.NextBool(3))
                CustomParticles.GenericMusicNotes(position, HarvestGold, 1, 25f);
        }

        /// <summary>HarvestMoon: Golden moon orb slams with radiant harvest energy.</summary>
        public static void HarvestMoonTelegraph(Vector2 targetArea)
        {
            Phase10BossVFX.StaffLineConvergence(targetArea + new Vector2(0, -180), HarvestGold, 0.8f);
            TelegraphSystem.DangerZone(targetArea, 180f, 30, HarvestGold * 0.3f);
        }

        public static void HarvestMoonImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(14f);
            CustomParticles.GenericFlare(position, HarvestGold, 1.2f, 20);
            CustomParticles.HaloRing(position, HarvestGold, 0.8f, 20);
            Phase10BossVFX.ChordResolutionBloom(position, new[] { HarvestGold }, 1.0f);
            BossSignatureVFX.AutumnLeafStorm(position, 1.2f);
        }

        #endregion

        #region Phase 2B Attack VFX (70% HP)

        /// <summary>SoulReap: Spectral sickle sweeps that drain life essence.</summary>
        public static void SoulReapTelegraph(Vector2 center)
        {
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f;
                TelegraphSystem.ThreatLine(center, angle.ToRotationVector2(), 400f, 25, WitheredRed * 0.6f);
            }
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { WitheredRed }, 0.7f);
        }

        public static void SoulReapImpact(Vector2 position)
        {
            CustomParticles.GlyphBurst(position, WitheredRed, 6);
            CustomParticles.HaloRing(position, DecayBrown, 0.6f, 18);
            Phase10BossVFX.SforzandoSpike(position, WitheredRed, 0.9f);
        }

        /// <summary>DecayingVortex: Spiral of withered leaves pulling inward.</summary>
        public static void DecayingVortexTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 150f, 8, DecayBrown * 0.5f);
            Phase10BossVFX.AccelerandoSpiral(center, DecayBrown, 0.8f);
        }

        public static void DecayingVortexBurst(Vector2 center, int waveIndex)
        {
            CustomParticles.ExplosionBurst(center, Color.Lerp(DecayBrown, AutumnOrange, waveIndex / 5f), 8 + waveIndex * 3);
            CustomParticles.GenericFlare(center, HarvestGold, 0.5f, 15);
            Phase10BossVFX.NoteConstellationWarning(center, AutumnOrange, 0.4f + waveIndex * 0.15f);
        }

        /// <summary>TwilightBarrage: Cascading amber projectiles like falling leaves at dusk.</summary>
        public static void TwilightBarrageTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            TelegraphSystem.ThreatLine(position, dir, 550f, 30, AutumnOrange * 0.7f);
            Phase10BossVFX.FortissimoFlashWarning(position, HarvestGold, 0.8f);
        }

        public static void TwilightBarrageParticle(Vector2 position)
        {
            Color color = Color.Lerp(AutumnOrange, HarvestGold, Main.rand.NextFloat(0.4f));
            CustomParticles.GenericFlare(position, color, 0.25f, 6);
            if (Main.rand.NextBool(4))
                CustomParticles.GenericMusicNotes(position, HarvestGold, 1, 20f);
        }

        #endregion

        #region Phase 2C Attack VFX (40% HP)

        /// <summary>AutumnalJudgment: Massive radial leaf storm with harvest rune impacts.</summary>
        public static void AutumnalJudgmentTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 200f, 12, HarvestGold * 0.7f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { HarvestGold }, 1.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, AutumnOrange, 1.2f);
        }

        public static void AutumnalJudgmentRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(18f);
            CustomParticles.GenericFlare(center, new Color(255, 220, 120), 1.2f, 20);
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Color color = i % 2 == 0 ? HarvestGold : AutumnOrange;
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 60f, color, 0.4f, 15);
            }
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { HarvestGold, AutumnOrange, WitheredRed }, 1.4f);
        }

        /// <summary>LastHarvest: Devastating diving attack trailing withered energy and decay.</summary>
        public static void LastHarvestTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitY);
            TelegraphSystem.ThreatLine(position, dir, 700f, 30, WitheredRed * 0.8f);
            TelegraphSystem.ImpactPoint(target, 60f, 30);
            Phase10BossVFX.CrescendoDangerRings(target, WitheredRed, 0.8f);
        }

        public static void LastHarvestImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(22f);
            CustomParticles.ExplosionBurst(position, AutumnOrange, 15);
            CustomParticles.GenericFlare(position, HarvestGold, 1.5f, 25);
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                CustomParticles.HaloRing(position + angle.ToRotationVector2() * 45f,
                    Color.Lerp(WitheredRed, HarvestGold, i / 8f), 0.4f, 15);
            }
            BossSignatureVFX.AutumnLeafStorm(position, 1.5f);
            Phase10BossVFX.TimpaniDrumrollImpact(position, AutumnOrange, 1.4f);
        }

        /// <summary>WitheringFinale: Ultimate spiral barrage finale with decaying crescendo.</summary>
        public static void WitheringFinaleTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 250f, 16, HarvestGold);
            Phase10BossVFX.StaffLineConvergence(center, HarvestGold, 1.2f);
            BossVFXOptimizer.WarningFlare(center, 1.0f);
        }

        public static void WitheringFinaleWave(Vector2 center, int waveIndex)
        {
            float angle = waveIndex * 0.15f;
            for (int arm = 0; arm < 5; arm++)
            {
                float armAngle = angle + MathHelper.TwoPi * arm / 5f;
                Vector2 pos = center + armAngle.ToRotationVector2() * 40f;
                Color color = Color.Lerp(HarvestGold, WitheredRed, arm / 5f);
                CustomParticles.GenericFlare(pos, color, 0.3f, 10);
            }
            CustomParticles.GenericMusicNotes(center, AutumnOrange, 3, 60f);
        }

        public static void WitheringFinaleEnd(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(25f);
            CustomParticles.GenericFlare(center, new Color(255, 220, 120), 2f, 30);
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Color color = VFXIntegration.GetThemeColor("Autumn", i / 20f);
                CustomParticles.GenericFlare(center + angle.ToRotationVector2() * 100f, color, 0.8f, 25);
            }
            Phase10BossVFX.CodaFinale(center, HarvestGold, WitheredRed, 2f);
        }

        #endregion
    }
}
