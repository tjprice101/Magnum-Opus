using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.OdeToJoy.Bosses.Systems
{
    /// <summary>
    /// Ode to Joy boss attack choreography system.
    /// 10 garden/nature-themed attacks with golden warmth.
    /// Phase 2 adds chromatic/rainbow elements to all effects.
    /// </summary>
    public static class OdeToJoyAttackVFX
    {
        private static readonly Color WarmGold = new Color(255, 200, 50);
        private static readonly Color RadiantAmber = new Color(240, 160, 40);
        private static readonly Color JubilantLight = new Color(255, 240, 200);
        private static readonly Color RosePink = new Color(230, 130, 150);

        #region Phase 1  EGarden Overture

        /// <summary>PetalStorm: Swirling cyclone of rose petals.</summary>
        public static void PetalStormTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 150f, 40, RosePink * 0.5f);
            Phase10BossVFX.AccelerandoSpiral(center, RosePink, 0.8f);
        }

        public static void PetalStormBurst(Vector2 center, int waveIndex)
        {
            Color color = Color.Lerp(RosePink, WarmGold, waveIndex / 5f);
            CustomParticles.GenericFlare(center, color, 0.4f + waveIndex * 0.08f, 14);
            CustomParticles.HaloRing(center, RosePink, 0.3f + waveIndex * 0.1f, 16);
            Phase10BossVFX.DynamicsWave(center, 0.4f + waveIndex * 0.1f, RosePink);
        }

        /// <summary>VineWhip: Lashing vine tendrils from the conductor's baton.</summary>
        public static void VineWhipTelegraph(Vector2 position, Vector2 direction)
        {
            TelegraphSystem.ThreatLine(position, direction, 450f, 25, WarmGold * 0.6f);
            BossVFXOptimizer.WarningFlare(position, 0.5f);
        }

        public static void VineWhipTrail(Vector2 position, Vector2 velocity)
        {
            CustomParticles.GenericFlare(position, WarmGold, 0.3f, 8);
            CustomParticles.GlowTrail(position, WarmGold, 0.3f);
        }

        public static void VineWhipImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(7f);
            CustomParticles.HaloRing(position, WarmGold, 0.5f, 15);
            CustomParticles.GenericFlare(position, RosePink, 0.5f, 12);
            Phase10BossVFX.ChordResolutionBloom(position, new[] { WarmGold, RosePink }, 0.7f);
        }

        /// <summary>RoseBudVolley: Budding projectiles that bloom on impact.</summary>
        public static void RoseBudVolleyTelegraph(Vector2 center)
        {
            Phase10BossVFX.NoteConstellationWarning(center, RosePink, 0.5f);
            TelegraphSystem.ConvergingRing(center, 100f, 30, RosePink * 0.4f);
        }

        public static void RoseBudVolleyBlooming(Vector2 position)
        {
            CustomParticles.GenericFlare(position, RosePink, 0.6f, 14);
            CustomParticles.HaloRing(position, WarmGold, 0.4f, 12);
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f + Main.rand.NextFloat(0.3f);
                CustomParticles.GenericFlare(position + angle.ToRotationVector2() * 20f,
                    Color.Lerp(RosePink, WarmGold, Main.rand.NextFloat()), 0.25f, 10);
            }
        }

        /// <summary>PollenCloud: Area denial golden pollen clouds.</summary>
        public static void PollenCloudTelegraph(Vector2 center)
        {
            TelegraphSystem.DangerZone(center, 180f, 40, WarmGold * 0.25f);
        }

        public static void PollenCloudPuff(Vector2 position)
        {
            Color color = Color.Lerp(WarmGold, RadiantAmber, Main.rand.NextFloat(0.4f));
            CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(30f, 30f),
                color * 0.6f, 0.25f, 20);
        }

        /// <summary>HarmonicBloom: Harmonic rings that expand outward in waves.</summary>
        public static void HarmonicBloomTelegraph(Vector2 center)
        {
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { WarmGold, RadiantAmber, JubilantLight }, 0.6f);
        }

        public static void HarmonicBloomRelease(Vector2 center, int ringIndex)
        {
            float scale = 0.5f + ringIndex * 0.15f;
            Color color = Color.Lerp(WarmGold, JubilantLight, ringIndex / 6f);
            CustomParticles.HaloRing(center, color, scale, 18);
            Phase10BossVFX.CrescendoRing(center, 30f + ringIndex * 40f, 200f, color);
        }

        #endregion

        #region Phase 2  EChromatic Symphony

        /// <summary>ChromaticCascade: Rainbow-hued projectile streams.</summary>
        public static void ChromaticCascadeTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 200f, 40, JubilantLight * 0.6f);
            Phase10BossVFX.FortissimoFlashWarning(center, JubilantLight, 1.0f);
        }

        public static void ChromaticCascadeProjectile(Vector2 position, int projectileIndex)
        {
            float hue = (projectileIndex * 0.1f) % 1f;
            Color rainbow = Main.hslToRgb(hue, 0.8f, 0.6f);
            CustomParticles.GenericFlare(position, rainbow, 0.35f, 12);
            CustomParticles.GlowTrail(position, rainbow * 0.6f, 0.25f);
        }

        public static void ChromaticCascadeImpact(Vector2 position, int projectileIndex)
        {
            float hue = (projectileIndex * 0.1f) % 1f;
            Color rainbow = Main.hslToRgb(hue, 0.8f, 0.6f);
            MagnumScreenEffects.AddScreenShake(6f);
            CustomParticles.HaloRing(position, rainbow, 0.5f, 14);
            CustomParticles.GenericFlare(position, JubilantLight, 0.4f, 12);
        }

        /// <summary>ThornyEmbrace: Thorn vines erupting from ground toward the player.</summary>
        public static void ThornyEmbraceTelegraph(Vector2 start, Vector2 end)
        {
            TelegraphSystem.LaserPath(start, end, 35f, 30, RadiantAmber * 0.5f);
            Phase10BossVFX.StaffLineLaser(start, end, RadiantAmber, 20f);
        }

        public static void ThornyEmbraceEruption(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(8f);
            CustomParticles.ExplosionBurst(position, RadiantAmber, 8, 4f);
            CustomParticles.HaloRing(position, WarmGold, 0.5f, 15);
        }

        /// <summary>GardenSymphony: Multi-area garden eruptions in sequence.</summary>
        public static void GardenSymphonyTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 250f, 50, WarmGold * 0.6f);
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { WarmGold, RadiantAmber, RosePink, JubilantLight }, 1.2f);
        }

        public static void GardenSymphonyBurst(Vector2 center, int burstIndex)
        {
            float angle = burstIndex * 0.2f;
            for (int i = 0; i < 6; i++)
            {
                float a = angle + MathHelper.TwoPi * i / 6f;
                Vector2 pos = center + a.ToRotationVector2() * (40f + burstIndex * 15f);
                Color color = Color.Lerp(WarmGold, RosePink, i / 6f);
                CustomParticles.GenericFlare(pos, color, 0.35f, 12);
            }
        }

        /// <summary>EternalBloom: Massive flower pattern explosion.</summary>
        public static void EternalBloomTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 300f, 60, JubilantLight * 0.7f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { WarmGold, RosePink, JubilantLight }, 1.0f);
        }

        public static void EternalBloomRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(20f);
            CustomParticles.GenericFlare(center, JubilantLight, 1.8f, 25);
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                float hue = (i / 12f);
                Color petalColor = Main.hslToRgb(hue, 0.7f, 0.65f);
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 60f, petalColor, 0.5f, 18);
            }
            Phase10BossVFX.CodaFinale(center, WarmGold, RosePink, 1.5f);
        }

        /// <summary>JubilantFinale: The ultimate joyous explosion  Erainbow beams in all directions.</summary>
        public static void JubilantFinaleTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 350f, 75, JubilantLight);
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { WarmGold, RadiantAmber, RosePink, JubilantLight }, 2.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, JubilantLight, 1.5f);
        }

        public static void JubilantFinaleRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(25f);
            CustomParticles.GenericFlare(center, JubilantLight, 2.5f, 30);
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                float hue = (i / 20f);
                Color color = Main.hslToRgb(hue, 0.9f, 0.6f);
                CustomParticles.GenericFlare(center + angle.ToRotationVector2() * 120f, color, 0.9f, 25);
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 80f, color, 0.5f, 20);
            }
            Phase10BossVFX.CodaFinale(center, JubilantLight, WarmGold, 2.5f);
            Phase10BossVFX.CadenceFinisher(center, new[] { WarmGold, RadiantAmber, RosePink, JubilantLight }, 1f);
        }

        #endregion
    }
}
