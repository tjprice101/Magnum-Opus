using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.DiesIrae.Bosses
{
    // #########################################################################
    //  DIES IRAE HERALD OF JUDGEMENT — BOSS VFX
    //  10 attack patterns, boss lifecycle, projectile VFX.
    //  The ultimate ecclesiastical wrath — day of judgment made manifest.
    // #########################################################################

    /// <summary>
    /// VFX for DiesIraeHeraldOfJudgement — ultimate Dies Irae boss.
    /// Comprehensive per-attack VFX for all 10 attack patterns across 3 tiers,
    /// boss lifecycle VFX (spawn, ambient, enrage, death),
    /// and per-projectile VFX.
    /// </summary>
    public static class DiesIraeHeraldOfJudgementVFX
    {
        // =====================================================================
        //  BOSS LIFECYCLE — SPAWN / AMBIENT / ENRAGE / DEATH
        // =====================================================================

        /// <summary>
        /// Boss spawn VFX — massive hellfire eruption announcing the Herald of Judgement.
        /// </summary>
        public static void BossSpawnVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.HellfireEruption(pos, 2.0f);

            // 5 cascading judgment rings
            for (int i = 0; i < 5; i++)
            {
                CustomParticles.DiesIraeHellfireBurst(pos, 15 + i * 5);
            }

            // 40-point radial wrath burst
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 14f);
                Color col = DiesIraePalette.GetFireGradient((float)i / 40f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 2.0f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnHeavySmoke(pos, 18, 1.5f, 7f, 100);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 10, 5f);
            DiesIraeVFXLibrary.SpawnMusicNotes(pos, 14, 70f, 0.8f, 1.4f, 55);

            MagnumScreenEffects.AddScreenShake(15f);
            Lighting.AddLight(pos, DiesIraePalette.WrathWhite.ToVector3() * 3.5f);
        }

        /// <summary>
        /// Boss ambient aura — hellfire judgment presence.
        /// Converging wrath motes, heavy smoke, bone ash drift, judgment shimmer ring.
        /// </summary>
        public static void BossAmbientAura(NPC npc, float auraIntensity)
        {
            if (Main.dedServ) return;

            // Converging wrath fire motes
            if (Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 60f + Main.rand.NextFloat(30f);
                Vector2 dustPos = npc.Center + angle.ToRotationVector2() * radius;
                Vector2 toCenter = (npc.Center - dustPos).SafeNormalize(Vector2.Zero) * 2f;
                Color col = Color.Lerp(DiesIraePalette.BloodRed, DiesIraePalette.JudgmentGold,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, toCenter, 80, col, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.0f;
            }

            // Heavy smoke atmosphere
            DiesIraeVFXLibrary.SpawnHeavySmoke(npc.Center, 2, 0.6f, 1.5f, 60);
            DiesIraeVFXLibrary.SpawnAmbientSmoke(npc.Center, 45f);

            // Rising bone ash
            if (Main.rand.NextBool(5))
                DiesIraeVFXLibrary.SpawnBoneAshScatter(npc.Center, 1, 1f);

            // Rising embers
            if (Main.rand.NextBool(4))
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -1.3f - Main.rand.NextFloat(0.7f));
                Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 100, col, 1.2f);
                d.noGravity = true;
            }

            // Judgment resonance ring (6-point)
            if (Main.rand.NextBool(6))
            {
                float time = (float)Main.timeForVisualEffects;
                for (int i = 0; i < 6; i++)
                {
                    float ringAngle = MathHelper.TwoPi * i / 6f + time * 0.01f;
                    Vector2 ringPos = npc.Center + ringAngle.ToRotationVector2() * 55f;
                    Dust d = Dust.NewDustPerfect(ringPos, DustID.Enchanted_Gold, Vector2.Zero, 0,
                        DiesIraePalette.JudgmentGold, 0.7f);
                    d.noGravity = true;
                }
            }

            // Music notes
            if (Main.rand.NextBool(15))
                DiesIraeVFXLibrary.SpawnMusicNotes(npc.Center, 1, 35f, 0.7f, 0.9f, 35);

            float pulse = 0.6f + auraIntensity * 0.4f;
            Lighting.AddLight(npc.Center, DiesIraePalette.InfernalRed.ToVector3() * pulse);
        }

        /// <summary>
        /// Boss enrage VFX — intensified hellfire eruption.
        /// </summary>
        public static void BossEnrageVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.HellfireEruption(pos, 1.8f);

            for (int i = 0; i < 3; i++)
            {
                CustomParticles.DiesIraeHellfireBurst(pos, 18 + i * 6);
            }

            DiesIraeVFXLibrary.SpawnEmberScatter(pos, 20, 8f);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 8, 4f);

            MagnumScreenEffects.AddScreenShake(12f);
            Lighting.AddLight(pos, DiesIraePalette.WrathWhite.ToVector3() * 2.5f);
        }

        /// <summary>
        /// Boss death VFX — ultimate hellfire judgment finale.
        /// </summary>
        public static void BossDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.HellfireEruption(pos, 3.0f);

            // 50-point radial fire burst (maximum scale)
            for (int i = 0; i < 50; i++)
            {
                float angle = MathHelper.TwoPi * i / 50f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(10f, 18f);
                Color col = DiesIraePalette.GetFireGradient((float)i / 50f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 2.2f);
                d.noGravity = true;
            }

            // Death judgment ring cascade
            for (int i = 0; i < 6; i++)
            {
                CustomParticles.DiesIraeHellfireBurst(pos, 20 + i * 5);
            }
            DiesIraeVFXLibrary.SpawnJudgmentRings(pos, 6, 0.6f);

            // Maximum heavy smoke
            DiesIraeVFXLibrary.SpawnHeavySmoke(pos, 22, 1.8f, 8f, 110);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 12, 5f);

            // Music notes finale
            DiesIraeVFXLibrary.SpawnMusicNotes(pos, 18, 90f, 0.8f, 1.5f, 60);

            MagnumScreenEffects.AddScreenShake(18f);
            Lighting.AddLight(pos, DiesIraePalette.WrathWhite.ToVector3() * 4f);
        }

        // =====================================================================
        //  ATTACK TELEGRAPHS / WARNINGS
        // =====================================================================

        /// <summary>
        /// Attack transition alert — judgment flash before each attack.
        /// </summary>
        public static void AttackAlertVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.DrawBloom(pos, 0.6f);
            CustomParticles.DiesIraeImpactBurst(pos, 8);
            DiesIraeVFXLibrary.SpawnMusicNotes(pos, 3, 25f, 0.8f, 1.0f, 25);
            DiesIraeVFXLibrary.SpawnEmberScatter(pos, 6, 3f);
            Lighting.AddLight(pos, DiesIraePalette.JudgmentGold.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Generic charge-up VFX — converging fire particles.
        /// </summary>
        public static void ChargeUpVFX(Vector2 pos, float progress)
        {
            if (Main.dedServ) return;

            int dustCount = 4 + (int)(progress * 8);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 60f * (1f - progress * 0.5f) + Main.rand.NextFloat(20f);
                Vector2 dustPos = pos + angle.ToRotationVector2() * radius;
                Vector2 toCenter = (pos - dustPos).SafeNormalize(Vector2.Zero) * (2f + progress * 4f);
                Color col = Color.Lerp(DiesIraePalette.BloodRed, DiesIraePalette.WrathWhite, progress);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, toCenter, 0, col, 1.0f + progress * 0.8f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnHeavySmoke(pos, 1 + (int)(progress * 3), 0.5f + progress * 0.4f, 2f, 40);
            Lighting.AddLight(pos, DiesIraePalette.InfernalRed.ToVector3() * (0.5f + progress * 0.5f));
        }

        /// <summary>
        /// Warning flare — pulsing red danger indicator at target positions.
        /// </summary>
        public static void WarningFlareVFX(Vector2 pos, float progress)
        {
            if (Main.dedServ) return;

            float pulse = 0.6f + MathF.Sin(progress * MathHelper.TwoPi * 4f) * 0.3f;
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustPos = pos + Main.rand.NextVector2Circular(8f * pulse, 8f * pulse);
                Color col = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.WrathWhite, progress);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, Vector2.Zero, 0, col, 0.8f * pulse);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, DiesIraePalette.InfernalRed.ToVector3() * 0.4f * pulse);
        }

        // =====================================================================
        //  ATTACK 1 — HELLFIRE BARRAGE
        // =====================================================================

        /// <summary>
        /// Hellfire Barrage cast VFX — fire fan spray origin.
        /// </summary>
        public static void HellfireBarrageCastVFX(Vector2 pos, Vector2 direction)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.DrawBloom(pos, 0.7f);

            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(3f, 6f)
                    + Main.rand.NextVector2Circular(2f, 2f);
                Color col = DiesIraePalette.GetWrathGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnMusicNotes(pos, 2, 15f, 0.8f, 1.0f, 20);
            Lighting.AddLight(pos, DiesIraePalette.InfernalRed.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  ATTACK 2 — JUDGMENT RAY
        // =====================================================================

        /// <summary>
        /// Judgment Ray telegraph VFX — sweeping line preview.
        /// </summary>
        public static void JudgmentRayTelegraphVFX(Vector2 startPos, Vector2 direction, float length)
        {
            if (Main.dedServ) return;

            int segmentCount = (int)(length / 30f);
            for (int i = 0; i < segmentCount; i++)
            {
                float t = (float)i / segmentCount;
                Vector2 pos = startPos + direction * (length * t);

                if (Main.rand.NextBool(2))
                {
                    Color col = DiesIraePalette.GetJudgmentGradient(t);
                    Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(5f, 5f),
                        DustID.Torch, Vector2.Zero, 0, col, 0.6f);
                    d.noGravity = true;
                }
            }
        }

        /// <summary>
        /// Judgment Ray fire VFX — active beam per-frame.
        /// </summary>
        public static void JudgmentRayBeamVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Color col = DiesIraePalette.GetJudgmentGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.Torch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, DiesIraePalette.JudgmentGold.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  ATTACK 3 — INFERNAL RING
        // =====================================================================

        /// <summary>
        /// Infernal Ring windup VFX — expanding fire symbols.
        /// </summary>
        public static void InfernalRingWindupVFX(Vector2 center, float progress)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;
            int ringCount = 8;
            for (int i = 0; i < ringCount; i++)
            {
                float angle = MathHelper.TwoPi * i / ringCount + time * 0.02f;
                float radius = 30f + progress * 30f;
                Vector2 pos = center + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(2))
                {
                    Color col = DiesIraePalette.GetWrathGradient(progress);
                    Dust d = Dust.NewDustPerfect(pos, DustID.Torch,
                        angle.ToRotationVector2() * 0.3f, 0, col, 0.9f + progress * 0.3f);
                    d.noGravity = true;
                }
            }
        }

        /// <summary>
        /// Infernal Ring burst — concentric ring release.
        /// </summary>
        public static void InfernalRingBurstVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.DrawBloom(pos, 0.8f);
            DiesIraeVFXLibrary.SpawnRadialDustBurst(pos, 16, 7f);
            DiesIraeVFXLibrary.SpawnMusicNotes(pos, 4, 25f, 0.8f, 1.1f, 30);
            CustomParticles.DiesIraeHellfireBurst(pos, 10);

            MagnumScreenEffects.AddScreenShake(5f);
            Lighting.AddLight(pos, DiesIraePalette.InfernalRed.ToVector3() * 1.0f);
        }

        // =====================================================================
        //  ATTACK 4 — CONDEMNATION STRIKE (dash)
        // =====================================================================

        /// <summary>
        /// Condemnation Strike dash trail VFX — fire trail during rapid dash.
        /// </summary>
        public static void CondemnationDashTrailVFX(NPC npc)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 4; i++)
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.4f, npc.height * 0.4f);
                Vector2 vel = -npc.velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f);
                Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.5f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnHeavySmoke(npc.Center - npc.velocity * 0.5f, 2, 0.7f, 2f, 35);
            DiesIraeVFXLibrary.SpawnEmberScatter(npc.Center, 2, 2.5f);
            Lighting.AddLight(npc.Center, DiesIraePalette.InfernalRed.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Condemnation Strike impact VFX.
        /// </summary>
        public static void CondemnationStrikeImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.WrathShockwaveImpact(pos, 0.8f);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 4, 3f);

            MagnumScreenEffects.AddScreenShake(6f);
            Lighting.AddLight(pos, DiesIraePalette.WrathWhite.ToVector3() * 1.2f);
        }

        // =====================================================================
        //  ATTACK 5 — SOUL HARVEST
        // =====================================================================

        /// <summary>
        /// Soul Harvest cast VFX — homing soul release burst.
        /// </summary>
        public static void SoulHarvestCastVFX(Vector2 pos, int soulCount)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.DrawBloom(pos, 0.6f);

            for (int i = 0; i < soulCount; i++)
            {
                float angle = MathHelper.TwoPi * i / soulCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = DiesIraePalette.GetJudgmentGradient((float)i / soulCount);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnMusicNotes(pos, 3, 20f, 0.8f, 1.0f, 25);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 3, 2f);
            Lighting.AddLight(pos, DiesIraePalette.DoomPurple.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  ATTACK 6 — WRATHFUL DESCENT (dive bomb)
        // =====================================================================

        /// <summary>
        /// Wrathful Descent dive trail VFX — fire trail during dive.
        /// </summary>
        public static void WrathfulDescentTrailVFX(NPC npc)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 5; i++)
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2Circular(15f, 15f);
                Vector2 vel = -npc.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 4f)
                    + Main.rand.NextVector2Circular(1f, 1f);
                Color col = DiesIraePalette.GetWrathGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.6f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnHeavySmoke(npc.Center, 3, 0.8f, 3f, 40);
            Lighting.AddLight(npc.Center, DiesIraePalette.InfernalRed.ToVector3() * 1.0f);
        }

        /// <summary>
        /// Wrathful Descent impact VFX — massive ground slam.
        /// </summary>
        public static void WrathfulDescentImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.FinisherSlam(pos, 1.2f);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 6, 4f);

            Lighting.AddLight(pos, DiesIraePalette.WrathWhite.ToVector3() * 1.5f);
        }

        // =====================================================================
        //  ATTACK 7 — CHAIN OF DAMNATION
        // =====================================================================

        /// <summary>
        /// Chain of Damnation cast VFX — chain projectile release.
        /// </summary>
        public static void ChainOfDamnationCastVFX(Vector2 pos, Vector2 direction)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(2f, 5f)
                    + Main.rand.NextVector2Circular(1f, 1f);
                Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.DrawBloom(pos, 0.4f);
            Lighting.AddLight(pos, DiesIraePalette.InfernalRed.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Chain link trail VFX — per-frame fire trail on chain projectiles.
        /// </summary>
        public static void ChainLinkTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f);
            Color col = DiesIraePalette.GetWrathGradient(Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.9f);
            d.noGravity = true;

            Lighting.AddLight(pos, DiesIraePalette.InfernalRed.ToVector3() * 0.4f);
        }

        // =====================================================================
        //  ATTACK 8 — APOCALYPSE RAIN
        // =====================================================================

        /// <summary>
        /// Apocalypse Rain warning — fire sparks above target area.
        /// </summary>
        public static void ApocalypseRainWarningVFX(Vector2 targetPos)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 4; i++)
            {
                Vector2 pos = targetPos + new Vector2(Main.rand.NextFloat(-40f, 40f), -70f - Main.rand.NextFloat(20f));
                Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, new Vector2(0, 0.5f), 0, col, 0.7f);
                d.noGravity = true;
            }

            Lighting.AddLight(targetPos + new Vector2(0, -70f), DiesIraePalette.InfernalRed.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Apocalypse Rain descent VFX — fire motes raining down.
        /// </summary>
        public static void ApocalypseRainDescentVFX(Vector2 spawnPos)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(3f, 5f));
                Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(spawnPos + Main.rand.NextVector2Circular(12f, 5f),
                    DustID.Torch, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            Lighting.AddLight(spawnPos, DiesIraePalette.InfernalRed.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  ATTACK 9 — FINAL JUDGMENT (safe-arc radial)
        // =====================================================================

        /// <summary>
        /// Final Judgment charge VFX — converging fire with safe zone hint.
        /// </summary>
        public static void FinalJudgmentChargeVFX(Vector2 center, float progress)
        {
            if (Main.dedServ) return;

            int pointCount = 12 + (int)(progress * 12);
            for (int i = 0; i < pointCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pointCount;
                float radius = 80f * (1f - progress * 0.4f);
                Vector2 ringPos = center + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(2))
                {
                    Vector2 toCenter = (center - ringPos).SafeNormalize(Vector2.Zero) * (1f + progress * 3f);
                    Color col = Color.Lerp(DiesIraePalette.BloodRed, DiesIraePalette.WrathWhite, progress);
                    Dust d = Dust.NewDustPerfect(ringPos, DustID.Torch, toCenter, 0, col, 1.0f + progress * 0.6f);
                    d.noGravity = true;
                }
            }

            DiesIraeVFXLibrary.SpawnHeavySmoke(center, 2 + (int)(progress * 4), 0.5f + progress * 0.4f, 2f, 40);
            Lighting.AddLight(center, DiesIraePalette.JudgmentGold.ToVector3() * (0.5f + progress * 0.5f));
        }

        /// <summary>
        /// Final Judgment release — massive radial burst with safe arc.
        /// </summary>
        public static void FinalJudgmentReleaseVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.WrathShockwaveImpact(pos, 1.5f);
            DiesIraeVFXLibrary.SpawnHeavySmoke(pos, 10, 1.1f, 5f, 65);
            DiesIraeVFXLibrary.SpawnEmberScatter(pos, 15, 7f);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 6, 3f);

            for (int i = 0; i < 28; i++)
            {
                float angle = MathHelper.TwoPi * i / 28f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(7f, 12f);
                Color col = DiesIraePalette.GetJudgmentGradient((float)i / 28f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.7f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnMusicNotes(pos, 8, 40f, 0.8f, 1.2f, 35);

            MagnumScreenEffects.AddScreenShake(8f);
            Lighting.AddLight(pos, DiesIraePalette.WrathWhite.ToVector3() * 2f);
        }

        // =====================================================================
        //  ATTACK 10 — DIVINE PUNISHMENT (ultimate multi-phase)
        // =====================================================================

        /// <summary>
        /// Divine Punishment windup — massive converging fire and bone ash.
        /// </summary>
        public static void DivinePunishmentWindupVFX(Vector2 center, float progress)
        {
            if (Main.dedServ) return;

            int dustCount = 8 + (int)(progress * 20);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 100f * (1f - progress * 0.6f) + Main.rand.NextFloat(25f);
                Vector2 dustPos = center + angle.ToRotationVector2() * radius;
                Vector2 toCenter = (center - dustPos).SafeNormalize(Vector2.Zero) * (3f + progress * 6f);
                Color col = Color.Lerp(DiesIraePalette.BloodRed, DiesIraePalette.WrathWhite, progress);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, toCenter, 0, col, 1.2f + progress * 0.8f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnHeavySmoke(center, 4 + (int)(progress * 6), 0.7f + progress * 0.6f, 4f, 55);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(center, (int)(progress * 4), 2f);

            if (progress > 0.3f)
                DiesIraeVFXLibrary.SpawnMusicNotes(center, 1 + (int)(progress * 4), 20f + progress * 25f,
                    0.7f, 1.0f + progress * 0.3f, 30);

            MagnumScreenEffects.AddScreenShake(2f + progress * 5f);
            Lighting.AddLight(center, DiesIraePalette.InfernalRed.ToVector3() * (0.6f + progress * 0.9f));
        }

        /// <summary>
        /// Divine Punishment dash phase — rapid dashes with intense fire trail.
        /// </summary>
        public static void DivinePunishmentDashVFX(NPC npc)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 6; i++)
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.5f, npc.height * 0.5f);
                Vector2 vel = -npc.velocity * 0.4f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color col = DiesIraePalette.GetWrathGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.8f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnHeavySmoke(npc.Center, 3, 0.9f, 3f, 40);
            DiesIraeVFXLibrary.SpawnEmberScatter(npc.Center, 3, 3f);
            Lighting.AddLight(npc.Center, DiesIraePalette.InfernalRed.ToVector3() * 1.2f);
        }

        /// <summary>
        /// Divine Punishment final explosion — ultimate detonation.
        /// </summary>
        public static void DivinePunishmentExplosionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.HellfireEruption(pos, 1.5f);

            for (int i = 0; i < 36; i++)
            {
                float angle = MathHelper.TwoPi * i / 36f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 14f);
                Color col = DiesIraePalette.GetFireGradient((float)i / 36f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 2.0f);
                d.noGravity = true;
            }

            for (int i = 0; i < 4; i++)
            {
                CustomParticles.DiesIraeHellfireBurst(pos, 15 + i * 5);
            }

            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 8, 4f);
            DiesIraeVFXLibrary.SpawnMusicNotes(pos, 10, 50f, 0.8f, 1.3f, 45);

            MagnumScreenEffects.AddScreenShake(15f);
            Lighting.AddLight(pos, DiesIraePalette.WrathWhite.ToVector3() * 3f);
        }

        // =====================================================================
        //  BOSS ON-HIT / RECOVERY
        // =====================================================================

        /// <summary>
        /// Boss on-hit VFX — fire burst when boss is struck.
        /// </summary>
        public static void BossHitVFX(NPC npc)
        {
            if (Main.dedServ) return;

            Vector2 pos = npc.Center;

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            DiesIraeVFXLibrary.SpawnEmberScatter(pos, 3, 2.5f);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 1, 1.5f);
        }

        /// <summary>
        /// Recovery shimmer — vulnerability visual during post-attack recovery.
        /// </summary>
        public static void RecoveryShimmerVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(3))
            {
                Color shimmer = DiesIraePalette.GetJudgmentShimmer((float)Main.timeForVisualEffects);
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(25f, 25f),
                    DustID.Enchanted_Gold, Vector2.Zero, 0, shimmer, 0.5f);
                d.noGravity = true;
            }
        }

        // =====================================================================
        //  BOSS PROJECTILE VFX
        // =====================================================================

        /// <summary>
        /// Generic hellfire bolt trail — used for InfernoHostileBolt and CultistBossFireBall.
        /// </summary>
        public static void HellfireBoltTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2.5f)
                + Main.rand.NextVector2Circular(0.4f, 0.4f);
            Color col = DiesIraePalette.GetWrathGradient(Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.1f);
            d.noGravity = true;

            Lighting.AddLight(pos, DiesIraePalette.InfernalRed.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Generic hellfire bolt impact VFX.
        /// </summary>
        public static void HellfireBoltImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.ProjectileImpact(pos, 0.5f);
            DiesIraeVFXLibrary.SpawnBoneAshScatter(pos, 2, 1.5f);
        }

        /// <summary>
        /// Homing soul trail — doom purple trail for soul harvest projectiles.
        /// </summary>
        public static void SoulTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f);
            Color col = Color.Lerp(DiesIraePalette.DoomPurple, DiesIraePalette.JudgmentGold,
                Main.rand.NextFloat(0.3f));
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.8f);
            d.noGravity = true;

            Lighting.AddLight(pos, DiesIraePalette.DoomPurple.ToVector3() * 0.3f);
        }

        /// <summary>
        /// Soul impact VFX.
        /// </summary>
        public static void SoulImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.ProjectileImpact(pos, 0.4f);
        }

        /// <summary>
        /// Chain of Damnation cursed flame trail.
        /// </summary>
        public static void CursedChainTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f);
            Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.9f);
            d.noGravity = true;

            Lighting.AddLight(pos, DiesIraePalette.InfernalRed.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Fireball trail — standard fire comet from ring/descent attacks.
        /// </summary>
        public static void FireballTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f)
                + Main.rand.NextVector2Circular(0.3f, 0.3f);
            Color col = DiesIraePalette.GetFireGradient(Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.0f);
            d.noGravity = true;

            Lighting.AddLight(pos, DiesIraePalette.EmberOrange.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Fireball impact VFX.
        /// </summary>
        public static void FireballImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            DiesIraeVFXLibrary.ProjectileImpact(pos, 0.5f);
            DiesIraeVFXLibrary.SpawnEmberScatter(pos, 3, 2f);
        }
    }

    // #########################################################################
    //  LEGACY COMPATIBILITY WRAPPER
    // #########################################################################

    /// <summary>
    /// Legacy shared VFX helper — delegates to DiesIraeHeraldOfJudgementVFX.
    /// </summary>
    public static class DiesIraeEnemyVFX
    {
        public static void BossAmbientAura(NPC npc, float auraIntensity) =>
            DiesIraeHeraldOfJudgementVFX.BossAmbientAura(npc, auraIntensity);

        public static void BossSpawnVFX(Vector2 pos) =>
            DiesIraeHeraldOfJudgementVFX.BossSpawnVFX(pos);

        public static void BossEnrageVFX(Vector2 pos) =>
            DiesIraeHeraldOfJudgementVFX.BossEnrageVFX(pos);

        public static void BossDeathVFX(Vector2 pos) =>
            DiesIraeHeraldOfJudgementVFX.BossDeathVFX(pos);

        public static void BossHitVFX(NPC npc) =>
            DiesIraeHeraldOfJudgementVFX.BossHitVFX(npc);

        public static void AttackAlertVFX(Vector2 pos) =>
            DiesIraeHeraldOfJudgementVFX.AttackAlertVFX(pos);

        public static void EnemyProjectileTrailVFX(Vector2 pos, Vector2 velocity) =>
            DiesIraeHeraldOfJudgementVFX.HellfireBoltTrailVFX(pos, velocity);

        public static void EnemyProjectileImpactVFX(Vector2 pos) =>
            DiesIraeHeraldOfJudgementVFX.HellfireBoltImpactVFX(pos);
    }
}
