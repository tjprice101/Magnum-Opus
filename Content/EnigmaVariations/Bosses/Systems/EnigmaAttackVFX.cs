using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.EnigmaVariations.Bosses.Systems
{
    /// <summary>
    /// Enigma Variations boss attack choreography system.
    /// Manages attack-specific VFX sequences, telegraphs,
    /// and multi-layered impact effects for each attack pattern.
    /// Theme: Void black, deep purple, eerie green  Emystery and dread.
    /// </summary>
    public static class EnigmaAttackVFX
    {
        private static readonly Color VoidBlack = new Color(15, 5, 25);
        private static readonly Color DeepPurple = new Color(100, 30, 150);
        private static readonly Color EerieGreen = new Color(80, 200, 100);
        private static readonly Color MysteryWhite = new Color(200, 180, 220);

        #region Phase 1 Attack VFX

        /// <summary>VoidLunge: Fast lunge from void with shadow trail.</summary>
        public static void VoidLungeTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            TelegraphSystem.ThreatLine(position, dir, 450f, 30, DeepPurple * 0.7f);
            Phase10BossVFX.GlissandoSlideWarning(position, target, DeepPurple, 0.5f);
            BossVFXOptimizer.WarningFlare(position, 0.5f);
        }

        public static void VoidLungeTrail(Vector2 position, Vector2 velocity)
        {
            CustomParticles.EnigmaEyeTrail(position, velocity, DeepPurple, 0.3f);
            CustomParticles.GenericGlow(position, VoidBlack, 0.3f, 12);
            if (Main.rand.NextBool(4))
                CustomParticles.Glyph(position, DeepPurple, 0.2f);
        }

        public static void VoidLungeImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(8f);
            ThemedParticles.EnigmaImpact(position, 0.8f);
            CustomParticles.EnigmaEyeImpact(position, position, EerieGreen, 0.5f);
            CustomParticles.HaloRing(position, DeepPurple, 0.5f, 16);
        }

        /// <summary>EyeVolley: Launches tracking eye projectiles at the player.</summary>
        public static void EyeVolleyTelegraph(Vector2 position)
        {
            CustomParticles.EnigmaEyeFormation(position, EerieGreen, 4, 60f);
            Phase10BossVFX.NoteConstellationWarning(position, DeepPurple, 0.5f);
            TelegraphSystem.ConvergingRing(position, 80f, 6, EerieGreen * 0.5f);
        }

        public static void EyeVolleyRelease(Vector2 position, Vector2 direction)
        {
            CustomParticles.EnigmaEyeGaze(position, EerieGreen, 0.5f, direction);
            CustomParticles.GenericFlare(position, EerieGreen, 0.4f, 12);
            ThemedParticles.EnigmaMusicNotes(position, 2, 25f);
        }

        /// <summary>ParadoxRing: Expanding ring of paradox energy.</summary>
        public static void ParadoxRingTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 120f, 8, DeepPurple * 0.6f);
            Phase10BossVFX.CrescendoDangerRings(center, DeepPurple, 0.7f);
        }

        public static void ParadoxRingRelease(Vector2 center, float radius)
        {
            int count = 12;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                Color color = ThemedParticles.GetEnigmaGradient(i / (float)count);
                CustomParticles.GenericFlare(pos, color, 0.35f, 14);
            }
            ThemedParticles.EnigmaShockwave(center, radius / 80f);
        }

        /// <summary>ShadowDash: Quick teleport-dash through void space.</summary>
        public static void ShadowDashTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            TelegraphSystem.ThreatLine(position, dir, 500f, 30, VoidBlack * 0.8f);
            Phase10BossVFX.FortissimoFlashWarning(position, DeepPurple, 0.7f);
        }

        public static void ShadowDashImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(6f);
            CustomParticles.EnigmaEyeExplosion(position, EerieGreen, 5, 4f);
            CustomParticles.GlyphBurst(position, DeepPurple, 6, 4f);
            ThemedParticles.EnigmaShockwave(position, 0.8f);
        }

        /// <summary>GlyphCircle: Summoning circle of rotating glyphs.</summary>
        public static void GlyphCircleTelegraph(Vector2 center)
        {
            CustomParticles.GlyphCircle(center, DeepPurple, 8, 80f, 0.03f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { DeepPurple, EerieGreen, VoidBlack }, 0.5f);
        }

        public static void GlyphCircleRelease(Vector2 center, int glyphIndex)
        {
            float angle = MathHelper.TwoPi * glyphIndex / 8f;
            Vector2 pos = center + angle.ToRotationVector2() * 80f;
            CustomParticles.GlyphBurst(pos, EerieGreen, 4, 5f);
            CustomParticles.GenericFlare(pos, DeepPurple, 0.5f, 14);
        }

        #endregion

        #region Phase 2 Attack VFX

        /// <summary>TendrilRise: Void tendrils rising from beneath the player.</summary>
        public static void TendrilRiseTelegraph(Vector2 position)
        {
            TelegraphSystem.ImpactPoint(position, 50f, 30);
            Phase10BossVFX.CrescendoDangerRings(position, EerieGreen, 0.6f);
        }

        public static void TendrilRiseRelease(Vector2 position)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 tendrilPos = position + new Vector2(Main.rand.NextFloat(-20f, 20f), i * -25f);
                Color color = ThemedParticles.GetEnigmaGradient(i / 6f);
                CustomParticles.GenericFlare(tendrilPos, color, 0.3f + i * 0.05f, 15);
                CustomParticles.GenericGlow(tendrilPos, EerieGreen * 0.6f, 0.25f, 18);
            }
            ThemedParticles.EnigmaMusicNotes(position, 3, 30f);
        }

        /// <summary>ParadoxWeb: Network of paradox lines trapping the player.</summary>
        public static void ParadoxWebTelegraph(Vector2 center, Vector2[] nodes)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                TelegraphSystem.ThreatLine(center, (nodes[i] - center).SafeNormalize(Vector2.UnitX),
                    (nodes[i] - center).Length(), 25, DeepPurple * 0.5f);
            }
            BossVFXOptimizer.DangerZoneRing(center, 180f, 16);
        }

        public static void ParadoxWebActivate(Vector2 nodePos)
        {
            CustomParticles.GlyphImpact(nodePos, DeepPurple, EerieGreen, 0.5f);
            CustomParticles.HaloRing(nodePos, EerieGreen, 0.4f, 15);
        }

        /// <summary>RealityFracture: Lines of broken reality across the screen.</summary>
        public static void RealityFractureTelegraph(Vector2 start, Vector2 end)
        {
            TelegraphSystem.LaserPath(start, end, 20f, 40, EerieGreen * 0.5f);
            BossVFXOptimizer.WarningLine(start, (end - start).SafeNormalize(Vector2.UnitX),
                (end - start).Length(), 10, WarningType.Danger);
        }

        public static void RealityFractureRelease(Vector2 start, Vector2 end)
        {
            Phase10BossVFX.StaffLineLaser(start, end, EerieGreen, 20f);
            FateRealityDistortion.TriggerScreenSlice(start, end, 0.5f, 20);
            ThemedParticles.EnigmaMusicNoteBurst(Vector2.Lerp(start, end, 0.5f), 6, 3f);
        }

        /// <summary>EyeOfTheVoid: Massive eye materializes and fires a beam.</summary>
        public static void EyeOfTheVoidTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 180f, 12, EerieGreen * 0.7f);
            CustomParticles.EnigmaEyeFormation(center, EerieGreen, 6, 90f);
            Phase10BossVFX.FortissimoFlashWarning(center, EerieGreen, 1.0f);
        }

        public static void EyeOfTheVoidRelease(Vector2 center, Vector2 target)
        {
            MagnumScreenEffects.AddScreenShake(15f);
            BossSignatureVFX.EnigmaVoidGaze(center, (target - center).SafeNormalize(Vector2.UnitX), 1.2f);
            Phase10BossVFX.HarmonicOvertoneBeam(center, target, DeepPurple);
            CustomParticles.EnigmaEyeGaze(center, EerieGreen, 0.8f, (target - center).SafeNormalize(Vector2.UnitX));
        }

        #endregion

        #region Phase 3 Attack VFX

        /// <summary>ParadoxMirror: Clone illusions  Emultiple fake bosses appear.</summary>
        public static void ParadoxMirrorSpawn(Vector2 clonePosition)
        {
            ThemedParticles.EnigmaShockwave(clonePosition, 0.8f);
            CustomParticles.EnigmaEyeExplosion(clonePosition, DeepPurple, 4, 3f);
            CustomParticles.GlyphBurst(clonePosition, EerieGreen, 4, 3f);
            Phase10BossVFX.KeyChangeFlash(clonePosition, VoidBlack, DeepPurple, 0.6f);
        }

        public static void ParadoxMirrorDeath(Vector2 clonePosition)
        {
            CustomParticles.EnigmaEyeExplosion(clonePosition, EerieGreen, 6, 5f);
            ThemedParticles.EnigmaImpact(clonePosition, 0.6f);
            Phase10BossVFX.DiminuendoFade(clonePosition, DeepPurple, 0.7f);
        }

        /// <summary>VoidImplosion: Collapsing void that pulls everything inward.</summary>
        public static void VoidImplosionTelegraph(Vector2 center, float radius)
        {
            TelegraphSystem.DangerZone(center, radius, 60, DeepPurple * 0.4f);
            Phase10BossVFX.CrescendoDangerRings(center, EerieGreen, 1.0f, 5);
            CustomParticles.EnigmaEyeOrbit(center, EerieGreen, 6, radius * 0.8f);
        }

        public static void VoidImplosionRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(18f);
            CustomParticles.GenericFlare(center, MysteryWhite, 1.5f, 25);
            ThemedParticles.EnigmaImpact(center, 1.5f);
            CustomParticles.GlyphBurst(center, EerieGreen, 10, 6f);
            Phase10BossVFX.SforzandoSpike(center, DeepPurple, 1.2f);
        }

        /// <summary>RealityUnravel: Ultimate attack  Ereality collapses with eyes and glyphs everywhere.</summary>
        public static void RealityUnravelTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 300f, 16, EerieGreen);
            Phase10BossVFX.StaffLineConvergence(center, DeepPurple, 1.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, EerieGreen, 1.5f);
            BossVFXOptimizer.WarningFlare(center, 1.2f);
        }

        public static void RealityUnravelRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(25f);
            FateRealityDistortion.TriggerRealityShatter(center, 8, 1.0f, 30);
            CustomParticles.GenericFlare(center, MysteryWhite, 2.0f, 30);
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Color color = ThemedParticles.GetEnigmaGradient(i / 16f);
                CustomParticles.GenericFlare(center + angle.ToRotationVector2() * 120f, color, 0.9f, 25);
                CustomParticles.EnigmaEyeGaze(center + angle.ToRotationVector2() * 100f, EerieGreen, 0.4f);
            }
            Phase10BossVFX.CodaFinale(center, DeepPurple, EerieGreen, 2.0f);
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { DeepPurple, EerieGreen, MysteryWhite }, 1.8f);
            BossSignatureVFX.EnigmaParadoxJudgment(center, 5, 5, 2.0f);
        }

        #endregion
    }
}
