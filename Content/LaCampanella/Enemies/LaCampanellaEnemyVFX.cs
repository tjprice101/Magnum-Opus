using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.LaCampanella.Enemies
{
    // #########################################################################
    //  CRAWLER OF THE BELL — VFX
    // #########################################################################

    /// <summary>
    /// VFX for CrawlerOfTheBell — La Campanella mini-boss enemy.
    /// 5 attack patterns, 6 projectile types, ambient, hit, death.
    /// </summary>
    public static class CrawlerOfTheBellVFX
    {
        // =====================================================================
        //  AMBIENT AURA
        // =====================================================================

        /// <summary>
        /// Per-frame ambient aura. Converging fire motes, smoke wisps,
        /// rising embers, bell shimmer, sparse music notes.
        /// </summary>
        public static void AmbientAura(NPC npc, float auraIntensity)
        {
            if (Main.dedServ) return;

            // Converging fire dust aura
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 45f + Main.rand.NextFloat(20f);
                Vector2 dustPos = npc.Center + angle.ToRotationVector2() * radius;
                Vector2 toCenter = (npc.Center - dustPos).SafeNormalize(Vector2.Zero) * 1.5f;
                Color col = Color.Lerp(LaCampanellaPalette.DeepEmber, LaCampanellaPalette.BellGold,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, toCenter, 80, col, 1.2f);
                d.noGravity = true;
                d.fadeIn = 1.0f;
            }

            // Ambient smoke wisps
            LaCampanellaVFXLibrary.SpawnAmbientSmoke(npc.Center, 35f);

            // Rising ember particles
            if (Main.rand.NextBool(6))
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -1f - Main.rand.NextFloat(0.5f));
                Color col = Color.Lerp(LaCampanellaPalette.EmberRed, LaCampanellaPalette.FlameYellow,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 100, col, 0.9f);
                d.noGravity = true;
            }

            // Enhanced particles during attack states
            if (auraIntensity > 0.5f && Main.rand.NextBool(4))
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = npc.Center + Main.rand.NextVector2Circular(20f, 20f);
                    Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(pos, DustID.Torch,
                        Main.rand.NextVector2Circular(1f, 1f), 0, col, 1.0f + auraIntensity * 0.3f);
                    d.noGravity = true;
                }
            }

            // Bell shimmer sparkles
            if (Main.rand.NextBool(10))
            {
                float time = (float)Main.timeForVisualEffects;
                Color shimmer = LaCampanellaPalette.GetBellShimmer(time);
                Dust d = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(30f, 30f),
                    DustID.Enchanted_Gold, Vector2.Zero, 0, shimmer, 0.5f);
                d.noGravity = true;
            }

            // Sparse music notes
            if (Main.rand.NextBool(20))
                LaCampanellaVFXLibrary.SpawnMusicNotes(npc.Center, 1, 25f, 0.7f, 0.9f, 30);

            float pulse = 0.5f + auraIntensity * 0.3f;
            Lighting.AddLight(npc.Center, LaCampanellaPalette.InfernalOrange.ToVector3() * pulse);
        }

        // =====================================================================
        //  ATTACK ALERT
        // =====================================================================

        /// <summary>
        /// Attack transition alert burst — fire flash with bell chime.
        /// </summary>
        public static void AlertBurst(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.DrawBloom(pos, 0.6f);
            CustomParticles.LaCampanellaBellChime(pos, 8);
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 3, 25f, 0.8f, 1.0f, 25);
            LaCampanellaVFXLibrary.SpawnEmberScatter(pos, 6, 3f);
            Lighting.AddLight(pos, LaCampanellaPalette.FlameYellow.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  ATTACK 1 — BELL TOLL FLAMES
        // =====================================================================

        /// <summary>
        /// Bell Toll Flames windup — converging fire dust toward cast origin.
        /// </summary>
        public static void BellTollFlamesWindupVFX(Vector2 castPos, float progress)
        {
            if (Main.dedServ) return;

            int dustCount = 3 + (int)(progress * 5);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 60f * (1f - progress * 0.5f) + Main.rand.NextFloat(20f);
                Vector2 dustPos = castPos + angle.ToRotationVector2() * radius;
                Vector2 toCenter = (castPos - dustPos).SafeNormalize(Vector2.Zero) * (2f + progress * 3f);
                Color col = LaCampanellaPalette.GetFireGradient(progress);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, toCenter, 0, col, 1.0f + progress * 0.5f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.SpawnHeavySmoke(castPos, 1 + (int)(progress * 3), 0.5f + progress * 0.3f, 2f, 40);
            Lighting.AddLight(castPos, LaCampanellaPalette.InfernalOrange.ToVector3() * (0.5f + progress * 0.5f));
        }

        /// <summary>
        /// Bell Toll Flames burst — radial fire eruption from bell toll.
        /// </summary>
        public static void BellTollFlamesBurstVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.BellShockwaveImpact(pos, 1.0f);

            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                Color col = LaCampanellaPalette.GetFireGradient((float)i / 16f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.5f);
                d.noGravity = true;
            }

            MagnumScreenEffects.AddScreenShake(5f);
        }

        // =====================================================================
        //  ATTACK 2 — INFERNAL CRAWL
        // =====================================================================

        /// <summary>
        /// Infernal Crawl per-frame trail — fire/smoke trail during charge.
        /// </summary>
        public static void InfernalCrawlTrailVFX(NPC npc)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 3; i++)
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.4f, npc.height * 0.4f);
                Vector2 vel = -npc.velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f);
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.4f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.SpawnHeavySmoke(npc.Center - npc.velocity * 0.5f, 2, 0.7f, 2f, 40);
            LaCampanellaVFXLibrary.SpawnEmberScatter(npc.Center, 2, 2f);
            Lighting.AddLight(npc.Center, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.7f);
        }

        // =====================================================================
        //  ATTACK 3 — SMOKE BURST
        // =====================================================================

        /// <summary>
        /// Smoke Burst eruption — massive smoke cloud with fire particles.
        /// </summary>
        public static void SmokeBurstVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 15, 1.2f, 5f, 80);

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.SpawnEmberScatter(pos, 10, 5f);
            LaCampanellaVFXLibrary.DrawBloom(pos, 0.5f);
            Lighting.AddLight(pos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  ATTACK 4 — CHIME PILLARS
        // =====================================================================

        /// <summary>
        /// Chime Pillars windup — bell symbols at pillar positions.
        /// </summary>
        public static void ChimePillarWindupVFX(Vector2 pillarPos, float progress)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 3; i++)
            {
                Vector2 pos = pillarPos + new Vector2(Main.rand.NextFloat(-10f, 10f), -Main.rand.NextFloat(30f) * progress);
                Vector2 vel = new Vector2(0, -2f - progress * 3f);
                Color col = LaCampanellaPalette.GetBellGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.0f + progress * 0.5f);
                d.noGravity = true;
            }

            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(pillarPos, DustID.GoldFlame, new Vector2(0, -1f), 0,
                    LaCampanellaPalette.BellGold, 0.8f);
                d.noGravity = true;
            }

            Lighting.AddLight(pillarPos, LaCampanellaPalette.BellGold.ToVector3() * (0.4f + progress * 0.4f));
        }

        /// <summary>
        /// Chime Pillar eruption — fire column burst.
        /// </summary>
        public static void ChimePillarEruptionVFX(Vector2 pillarPos)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 20; i++)
            {
                float height = i * 8f;
                Vector2 pos = pillarPos + new Vector2(Main.rand.NextFloat(-8f, 8f), -height);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -3f - Main.rand.NextFloat(4f));
                Color col = LaCampanellaPalette.GetFireGradient((float)i / 20f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.5f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.DrawBloom(pillarPos, 0.6f);
            CustomParticles.LaCampanellaBellChime(pillarPos, 8);
            LaCampanellaVFXLibrary.SpawnMusicNotes(pillarPos, 4, 15f, 0.8f, 1.1f, 40);
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pillarPos, 4, 0.7f, 2f, 40);

            MagnumScreenEffects.AddScreenShake(3f);
            Lighting.AddLight(pillarPos, LaCampanellaPalette.WhiteHot.ToVector3() * 1.0f);
        }

        // =====================================================================
        //  ATTACK 5 — CRESCENDO INFERNO
        // =====================================================================

        /// <summary>
        /// Crescendo Inferno warning — expanding fire ring.
        /// </summary>
        public static void CrescendoInfernoWarningVFX(Vector2 pos, float warningRadius, float progress)
        {
            if (Main.dedServ) return;

            int pointCount = 16 + (int)(progress * 8);
            for (int i = 0; i < pointCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pointCount;
                float radius = warningRadius * progress;
                Vector2 ringPos = pos + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(2))
                {
                    Color col = Color.Lerp(LaCampanellaPalette.DeepEmber, LaCampanellaPalette.FlameYellow, progress);
                    Dust d = Dust.NewDustPerfect(ringPos, DustID.Torch,
                        angle.ToRotationVector2() * 0.5f, 0, col, 0.8f + progress * 0.4f);
                    d.noGravity = true;
                }
            }

            if (Main.rand.NextBool(2))
                LaCampanellaVFXLibrary.SpawnEmberScatter(pos, 2 + (int)(progress * 4), 2f);

            Lighting.AddLight(pos, LaCampanellaPalette.InfernalOrange.ToVector3() * (0.5f + progress * 0.5f));
        }

        /// <summary>
        /// Crescendo Inferno impact — massive eruption.
        /// </summary>
        public static void CrescendoInfernoImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.InfernalEruption(pos, 1.2f);

            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 12f);
                Color col = LaCampanellaPalette.GetFireGradient((float)i / 24f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.8f);
                d.noGravity = true;
            }
        }

        // =====================================================================
        //  CRAWLER HIT / DEATH
        // =====================================================================

        /// <summary>
        /// On-hit VFX for Crawler of the Bell.
        /// </summary>
        public static void HitVFX(NPC npc)
        {
            if (Main.dedServ) return;

            Vector2 pos = npc.Center;

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.SpawnEmberScatter(pos, 3, 2.5f);

            Dust shimmer = Dust.NewDustPerfect(pos, DustID.GoldFlame,
                Main.rand.NextVector2Circular(2f, 2f), 0, LaCampanellaPalette.BellGold, 0.8f);
            shimmer.noGravity = true;
        }

        /// <summary>
        /// Death VFX for Crawler of the Bell.
        /// </summary>
        public static void DeathVFX(NPC npc)
        {
            if (Main.dedServ) return;

            Vector2 pos = npc.Center;

            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color col = LaCampanellaPalette.GetFireGradient((float)i / 30f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.5f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.DrawBloom(pos, 0.9f);
            LaCampanellaVFXLibrary.SpawnBellChimeRings(pos, 4, 0.4f);
            CustomParticles.LaCampanellaBellChime(pos, 15);
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 10, 1.0f, 4f, 70);
            LaCampanellaVFXLibrary.SpawnEmberScatter(pos, 15, 5f);
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 8, 40f, 0.8f, 1.2f, 40);

            MagnumScreenEffects.AddScreenShake(5f);
            Lighting.AddLight(pos, LaCampanellaPalette.WhiteHot.ToVector3() * 1.5f);
        }

        // =====================================================================
        //  CRAWLER PROJECTILE VFX
        // =====================================================================

        /// <summary>
        /// BellTollFlame projectile trail — fire comet with orange-gold trail.
        /// </summary>
        public static void BellTollFlameTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2.5f)
                + Main.rand.NextVector2Circular(0.4f, 0.4f);
            Color col = Color.Lerp(LaCampanellaPalette.InfernalOrange, LaCampanellaPalette.BellGold,
                Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.1f);
            d.noGravity = true;

            Lighting.AddLight(pos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.5f);
        }

        /// <summary>
        /// BellTollFlame impact — fire burst on death.
        /// </summary>
        public static void BellTollFlameImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;
            LaCampanellaVFXLibrary.ProjectileImpact(pos, 0.5f);
        }

        /// <summary>
        /// InfernalCrawlTrail projectile — stationary lingering fire patch.
        /// </summary>
        public static void InfernalCrawlTrailPatchVFX(Vector2 pos, float fadeProgress)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(3))
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.5f);
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(8f, 4f),
                    DustID.Torch, vel, 0, col, 1.2f * (1f - fadeProgress));
                d.noGravity = true;
            }

            Lighting.AddLight(pos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.4f * (1f - fadeProgress));
        }

        /// <summary>
        /// SmokeCloudProjectile trail — dense smoke with sparse embers.
        /// </summary>
        public static void SmokeCloudTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 1, 0.5f, 1.5f, 30);

            if (Main.rand.NextBool(4))
            {
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.Torch, -velocity * 0.1f, 0, col, 0.8f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// SmokeFireball trail — small fire comet spawned from smoke cloud.
        /// </summary>
        public static void SmokeFireballTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f);
            Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.9f);
            d.noGravity = true;

            Lighting.AddLight(pos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.3f);
        }

        /// <summary>
        /// SmokeFireball impact — small fire pop.
        /// </summary>
        public static void SmokeFireballImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;
            LaCampanellaVFXLibrary.ProjectileImpact(pos, 0.4f);
        }

        /// <summary>
        /// ChimePillar telegraph — ground warning sparks before eruption.
        /// </summary>
        public static void ChimePillarTelegraphVFX(Vector2 groundPos, float progress)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(2))
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -1f - progress * 2f);
                Color col = Color.Lerp(LaCampanellaPalette.DeepEmber, LaCampanellaPalette.BellGold, progress);
                Dust d = Dust.NewDustPerfect(groundPos + new Vector2(Main.rand.NextFloat(-10f, 10f), 0),
                    DustID.Torch, vel, 0, col, 0.8f + progress * 0.4f);
                d.noGravity = true;
            }

            Lighting.AddLight(groundPos, LaCampanellaPalette.BellGold.ToVector3() * (0.3f + progress * 0.3f));
        }

        /// <summary>
        /// CrescendoFireball trail — intense fire comet (ultimate attack projectile).
        /// </summary>
        public static void CrescendoFireballTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1.5f, 3f)
                    + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            if (Main.rand.NextBool(3))
                LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 1, 0.3f, 1f, 20);

            Lighting.AddLight(pos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.5f);
        }

        /// <summary>
        /// CrescendoFireball impact — intense fire burst (ultimate attack).
        /// </summary>
        public static void CrescendoFireballImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;
            LaCampanellaVFXLibrary.ProjectileImpact(pos, 0.8f);
            LaCampanellaVFXLibrary.SpawnEmberScatter(pos, 4, 3f);
        }
    }

    // #########################################################################
    //  CHIME OF LIFE (BOSS) — VFX
    // #########################################################################

    /// <summary>
    /// VFX for Chime of Life — La Campanella boss.
    /// Comprehensive per-attack VFX for all 14 attack patterns across 3 phases,
    /// boss lifecycle VFX (spawn, phase transitions, enrage, death),
    /// and per-projectile VFX for 5 boss projectile types.
    /// </summary>
    public static class ChimeOfLifeVFX
    {
        // =====================================================================
        //  BOSS LIFECYCLE — SPAWN / AMBIENT / PHASE / ENRAGE / DEATH
        // =====================================================================

        /// <summary>
        /// Boss spawn VFX — massive infernal eruption announcing the Chime of Life.
        /// </summary>
        public static void BossSpawnVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.InfernalEruption(pos, 1.8f);

            for (int i = 0; i < 5; i++)
            {
                CustomParticles.LaCampanellaBellChime(pos, 15 + i * 5);
            }

            // 40-point radial fire burst
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 14f);
                Color col = LaCampanellaPalette.GetFireGradient((float)i / 40f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 2.0f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 16, 1.5f, 6f, 90);
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 12, 60f, 0.8f, 1.3f, 50);

            MagnumScreenEffects.AddScreenShake(12f);
            Lighting.AddLight(pos, LaCampanellaPalette.WhiteHot.ToVector3() * 3f);
        }

        /// <summary>
        /// Boss ambient aura — enhanced with bell resonance ring and heavy smoke.
        /// </summary>
        public static void BossAmbientAura(NPC npc, float auraIntensity)
        {
            if (Main.dedServ) return;

            // Converging fire motes (boss-enhanced)
            if (Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 55f + Main.rand.NextFloat(25f);
                Vector2 dustPos = npc.Center + angle.ToRotationVector2() * radius;
                Vector2 toCenter = (npc.Center - dustPos).SafeNormalize(Vector2.Zero) * 2f;
                Color col = Color.Lerp(LaCampanellaPalette.DeepEmber, LaCampanellaPalette.BellGold,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, toCenter, 80, col, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.0f;
            }

            // Heavy smoke atmosphere
            LaCampanellaVFXLibrary.SpawnHeavySmoke(npc.Center, 2, 0.6f, 1.5f, 60);
            LaCampanellaVFXLibrary.SpawnAmbientSmoke(npc.Center, 40f);

            // Rising embers
            if (Main.rand.NextBool(4))
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2Circular(35f, 35f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -1.2f - Main.rand.NextFloat(0.6f));
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 100, col, 1.1f);
                d.noGravity = true;
            }

            // Boss bell resonance ring (6-point)
            if (Main.rand.NextBool(6))
            {
                float time = (float)Main.timeForVisualEffects;
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + time * 0.01f;
                    Vector2 ringPos = npc.Center + angle.ToRotationVector2() * 50f;
                    Dust d = Dust.NewDustPerfect(ringPos, DustID.GoldFlame, Vector2.Zero, 0,
                        LaCampanellaPalette.BellGold, 0.7f);
                    d.noGravity = true;
                }
            }

            // Music notes
            if (Main.rand.NextBool(15))
                LaCampanellaVFXLibrary.SpawnMusicNotes(npc.Center, 1, 30f, 0.7f, 0.9f, 35);

            float pulse = 0.6f + auraIntensity * 0.4f;
            Lighting.AddLight(npc.Center, LaCampanellaPalette.InfernalOrange.ToVector3() * pulse);
        }

        /// <summary>
        /// Boss phase transition VFX — massive bell shockwave + fire eruption.
        /// Called at 65% and 35% HP thresholds.
        /// </summary>
        public static void BossPhaseTransitionVFX(Vector2 pos, int toPhase)
        {
            if (Main.dedServ) return;

            float intensity = toPhase == 2 ? 1.5f : 2.0f;
            LaCampanellaVFXLibrary.InfernalEruption(pos, intensity);

            int bellCount = toPhase == 2 ? 3 : 5;
            for (int i = 0; i < bellCount; i++)
            {
                CustomParticles.LaCampanellaBellChime(pos, 15 + i * 5);
            }

            // Phase-appropriate fire ring
            int ringPoints = toPhase == 2 ? 24 : 32;
            for (int i = 0; i < ringPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / ringPoints;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f + toPhase * 2f);
                Color col = LaCampanellaPalette.GetFireGradient((float)i / ringPoints);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.6f + toPhase * 0.2f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 10 + toPhase * 3, 1.2f, 5f, 75);
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 8 + toPhase * 2, 50f, 0.8f, 1.2f, 40);

            MagnumScreenEffects.AddScreenShake(8f + toPhase * 2f);
            Lighting.AddLight(pos, LaCampanellaPalette.WhiteHot.ToVector3() * (1.5f + toPhase * 0.5f));
        }

        /// <summary>
        /// Boss enrage VFX — intensified fire eruption marking enrage state.
        /// </summary>
        public static void BossEnrageVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.InfernalEruption(pos, 1.8f);

            for (int i = 0; i < 3; i++)
            {
                CustomParticles.LaCampanellaBellChime(pos, 18 + i * 6);
            }

            // Intense ember spray
            LaCampanellaVFXLibrary.SpawnEmberScatter(pos, 20, 8f);

            MagnumScreenEffects.AddScreenShake(10f);
            Lighting.AddLight(pos, LaCampanellaPalette.WhiteHot.ToVector3() * 2f);
        }

        /// <summary>
        /// Boss death VFX — ultimate infernal finale.
        /// </summary>
        public static void BossDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.InfernalEruption(pos, 2.5f);

            // 50-point radial fire burst (maximum scale)
            for (int i = 0; i < 50; i++)
            {
                float angle = MathHelper.TwoPi * i / 50f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(10f, 16f);
                Color col = LaCampanellaPalette.GetFireGradient((float)i / 50f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 2.2f);
                d.noGravity = true;
            }

            // Death bell toll cascade
            for (int i = 0; i < 6; i++)
            {
                CustomParticles.LaCampanellaBellChime(pos, 20 + i * 5);
            }
            LaCampanellaVFXLibrary.SpawnBellChimeRings(pos, 6, 0.5f);

            // Maximum heavy smoke
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 20, 1.6f, 8f, 100);

            // Music notes finale
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 16, 80f, 0.8f, 1.4f, 55);

            MagnumScreenEffects.AddScreenShake(15f);
            Lighting.AddLight(pos, LaCampanellaPalette.WhiteHot.ToVector3() * 4f);
        }

        // =====================================================================
        //  PHASE 1 ATTACKS — BELL SLAM / TOLL WAVE / EMBER SHOWER
        // =====================================================================

        /// <summary>
        /// Bell Slam windup — fire converging toward slam target position.
        /// </summary>
        public static void BellSlamWindupVFX(Vector2 targetPos, float progress)
        {
            if (Main.dedServ) return;

            // Warning ring at target
            int pointCount = 8 + (int)(progress * 8);
            for (int i = 0; i < pointCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pointCount;
                float radius = 40f * (1f - progress * 0.3f);
                Vector2 ringPos = targetPos + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(2))
                {
                    Color col = Color.Lerp(LaCampanellaPalette.DeepEmber, LaCampanellaPalette.FlameYellow, progress);
                    Dust d = Dust.NewDustPerfect(ringPos, DustID.Torch, Vector2.Zero, 0, col, 0.8f + progress * 0.4f);
                    d.noGravity = true;
                }
            }

            Lighting.AddLight(targetPos, LaCampanellaPalette.InfernalOrange.ToVector3() * (0.4f + progress * 0.4f));
        }

        /// <summary>
        /// Bell Slam impact — shockwave at impact point.
        /// </summary>
        public static void BellSlamImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.BellShockwaveImpact(pos, 1.2f);
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 6, 0.9f, 4f, 50);
            LaCampanellaVFXLibrary.SpawnEmberScatter(pos, 8, 5f);
            CustomParticles.LaCampanellaBellChime(pos, 10);

            MagnumScreenEffects.AddScreenShake(6f);
            Lighting.AddLight(pos, LaCampanellaPalette.WhiteHot.ToVector3() * 1.2f);
        }

        /// <summary>
        /// Toll Wave charge VFX — rotating fire ring building up.
        /// </summary>
        public static void TollWaveChargeVFX(Vector2 bossCenter, float progress)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;
            int pointCount = 8 + (int)(progress * 8);
            for (int i = 0; i < pointCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pointCount + time * 0.03f;
                float radius = 40f + progress * 20f;
                Vector2 ringPos = bossCenter + angle.ToRotationVector2() * radius;

                Color col = LaCampanellaPalette.GetFireGradient(progress * ((float)i / pointCount));
                Dust d = Dust.NewDustPerfect(ringPos, DustID.Torch,
                    angle.ToRotationVector2() * 0.5f, 0, col, 0.9f + progress * 0.3f);
                d.noGravity = true;
            }

            Lighting.AddLight(bossCenter, LaCampanellaPalette.InfernalOrange.ToVector3() * (0.5f + progress * 0.3f));
        }

        /// <summary>
        /// Toll Wave burst — radial projectile release with shockwave.
        /// </summary>
        public static void TollWaveBurstVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.DrawBloom(pos, 0.8f);
            LaCampanellaVFXLibrary.SpawnRadialDustBurst(pos, 16, 6f);
            CustomParticles.LaCampanellaBellChime(pos, 10);
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 4, 20f, 0.8f, 1.0f, 25);

            MagnumScreenEffects.AddScreenShake(4f);
            Lighting.AddLight(pos, LaCampanellaPalette.FlameYellow.ToVector3() * 1.0f);
        }

        /// <summary>
        /// Ember Shower warning — fire sparks above target area.
        /// </summary>
        public static void EmberShowerWarningVFX(Vector2 targetPos)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 3; i++)
            {
                Vector2 pos = targetPos + new Vector2(Main.rand.NextFloat(-30f, 30f), -60f - Main.rand.NextFloat(20f));
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, new Vector2(0, 0.5f), 0, col, 0.7f);
                d.noGravity = true;
            }

            Lighting.AddLight(targetPos + new Vector2(0, -60f), LaCampanellaPalette.InfernalOrange.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Ember Shower rain VFX — fire motes descending.
        /// </summary>
        public static void EmberShowerRainVFX(Vector2 spawnPos)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(2f, 4f));
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(spawnPos + Main.rand.NextVector2Circular(10f, 5f),
                    DustID.Torch, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            Lighting.AddLight(spawnPos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  PHASE 2 ATTACKS
        // =====================================================================

        /// <summary>
        /// Fire Wall Sweep warning — fire line preview before wall sweep.
        /// </summary>
        public static void FireWallSweepWarningVFX(Vector2 startPos, Vector2 endPos, float progress)
        {
            if (Main.dedServ) return;

            int segmentCount = 8;
            for (int i = 0; i < segmentCount; i++)
            {
                float t = (float)i / segmentCount;
                Vector2 segPos = Vector2.Lerp(startPos, endPos, t);

                if (Main.rand.NextBool(2))
                {
                    Color col = Color.Lerp(LaCampanellaPalette.DeepEmber, LaCampanellaPalette.FlameYellow, progress);
                    Dust d = Dust.NewDustPerfect(segPos + Main.rand.NextVector2Circular(5f, 5f),
                        DustID.Torch, new Vector2(0, -0.5f), 0, col, 0.7f + progress * 0.3f);
                    d.noGravity = true;
                }
            }
        }

        /// <summary>
        /// Fire Wall Sweep burst — wall of fire materializing.
        /// </summary>
        public static void FireWallSweepBurstVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.DrawBloom(pos, 0.5f);
            LaCampanellaVFXLibrary.SpawnEmberScatter(pos, 6, 4f);
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 4, 0.7f, 3f, 40);

            MagnumScreenEffects.AddScreenShake(4f);
            Lighting.AddLight(pos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Chime Rings windup — expanding fire symbols.
        /// </summary>
        public static void ChimeRingsWindupVFX(Vector2 bossCenter, float progress)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;
            int ringCount = 6;
            for (int i = 0; i < ringCount; i++)
            {
                float angle = MathHelper.TwoPi * i / ringCount + time * 0.02f;
                float radius = 30f + progress * 30f;
                Vector2 pos = bossCenter + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(2))
                {
                    Dust d = Dust.NewDustPerfect(pos, DustID.Torch,
                        angle.ToRotationVector2() * 0.3f, 0,
                        LaCampanellaPalette.GetBellGradient(progress), 0.9f);
                    d.noGravity = true;
                }
            }
        }

        /// <summary>
        /// Chime Rings burst — expanding projectile ring release.
        /// </summary>
        public static void ChimeRingsBurstVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.DrawBloom(pos, 0.6f);
            CustomParticles.LaCampanellaBellChime(pos, 8);
            LaCampanellaVFXLibrary.SpawnRadialDustBurst(pos, 12, 5f);
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 3, 15f, 0.7f, 0.9f, 25);

            Lighting.AddLight(pos, LaCampanellaPalette.FlameYellow.ToVector3() * 0.9f);
        }

        /// <summary>
        /// Inferno Circle charge — fire spiral arms building.
        /// </summary>
        public static void InfernoCircleChargeVFX(Vector2 bossCenter, float progress)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;
            int armCount = 3;
            for (int arm = 0; arm < armCount; arm++)
            {
                float baseAngle = time * 0.04f + MathHelper.TwoPi * arm / armCount;
                int dustPerArm = 3 + (int)(progress * 4);
                for (int i = 0; i < dustPerArm; i++)
                {
                    float dist = 20f + i * 15f * progress;
                    float angle = baseAngle + i * 0.15f;
                    Vector2 pos = bossCenter + angle.ToRotationVector2() * dist;

                    Color col = LaCampanellaPalette.GetFireGradient((float)i / dustPerArm);
                    Dust d = Dust.NewDustPerfect(pos, DustID.Torch,
                        angle.ToRotationVector2() * 0.3f, 0, col, 0.8f + progress * 0.3f);
                    d.noGravity = true;
                }
            }

            Lighting.AddLight(bossCenter, LaCampanellaPalette.InfernalOrange.ToVector3() * (0.5f + progress * 0.3f));
        }

        /// <summary>
        /// Inferno Circle burst — spiral projectile release.
        /// </summary>
        public static void InfernoCircleBurstVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.DrawBloom(pos, 0.7f);
            LaCampanellaVFXLibrary.SpawnRadialDustBurst(pos, 18, 6f);
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 5, 0.8f, 3f, 45);

            MagnumScreenEffects.AddScreenShake(4f);
            Lighting.AddLight(pos, LaCampanellaPalette.FlameYellow.ToVector3() * 1.0f);
        }

        /// <summary>
        /// Rhythmic Toll pattern preview — musical pattern visualization.
        /// </summary>
        public static void RhythmicTollPreviewVFX(Vector2 bossCenter, int patternType)
        {
            if (Main.dedServ) return;

            int pointCount = patternType switch
            {
                0 => 12, // expanding ring
                1 => 8,  // dual offset rings
                2 => 10, // star burst
                _ => 8,  // spiral wave
            };

            for (int i = 0; i < pointCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pointCount;
                Vector2 pos = bossCenter + angle.ToRotationVector2() * 35f;
                Color col = LaCampanellaPalette.GetBellGradient((float)i / pointCount);
                Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame,
                    angle.ToRotationVector2() * 0.2f, 0, col, 0.7f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.SpawnMusicNotes(bossCenter, 2, 15f, 0.7f, 0.9f, 25);
        }

        /// <summary>
        /// Rhythmic Toll burst — musical projectile release.
        /// </summary>
        public static void RhythmicTollBurstVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.DrawBloom(pos, 0.7f);
            CustomParticles.LaCampanellaBellChime(pos, 10);
            LaCampanellaVFXLibrary.SpawnRadialDustBurst(pos, 14, 5f);
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 5, 25f, 0.8f, 1.1f, 30);

            MagnumScreenEffects.AddScreenShake(3f);
            Lighting.AddLight(pos, LaCampanellaPalette.BellGold.ToVector3() * 1.0f);
        }

        /// <summary>
        /// Infernal Judgment charge — converging fire with safe zone preview.
        /// </summary>
        public static void InfernalJudgmentChargeVFX(Vector2 bossCenter, float progress)
        {
            if (Main.dedServ) return;

            // Converging fire ring
            int pointCount = 12 + (int)(progress * 12);
            for (int i = 0; i < pointCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pointCount;
                float radius = 80f * (1f - progress * 0.4f);
                Vector2 ringPos = bossCenter + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(2))
                {
                    Vector2 toCenter = (bossCenter - ringPos).SafeNormalize(Vector2.Zero) * (1f + progress * 2f);
                    Color col = Color.Lerp(LaCampanellaPalette.DeepEmber, LaCampanellaPalette.WhiteHot, progress);
                    Dust d = Dust.NewDustPerfect(ringPos, DustID.Torch, toCenter, 0, col, 1.0f + progress * 0.5f);
                    d.noGravity = true;
                }
            }

            LaCampanellaVFXLibrary.SpawnHeavySmoke(bossCenter, 2 + (int)(progress * 3), 0.5f + progress * 0.3f, 2f, 40);
            Lighting.AddLight(bossCenter, LaCampanellaPalette.InfernalOrange.ToVector3() * (0.5f + progress * 0.5f));
        }

        /// <summary>
        /// Infernal Judgment release — massive fire shockwave with safe arc.
        /// </summary>
        public static void InfernalJudgmentReleaseVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.BellShockwaveImpact(pos, 1.4f);
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 8, 1.0f, 5f, 60);
            LaCampanellaVFXLibrary.SpawnEmberScatter(pos, 12, 6f);

            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                Color col = LaCampanellaPalette.GetFireGradient((float)i / 24f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.6f);
                d.noGravity = true;
            }

            CustomParticles.LaCampanellaBellChime(pos, 12);
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 6, 30f, 0.8f, 1.1f, 35);

            MagnumScreenEffects.AddScreenShake(7f);
            Lighting.AddLight(pos, LaCampanellaPalette.WhiteHot.ToVector3() * 1.5f);
        }

        /// <summary>
        /// Bell Laser Grid warning — laser path preview line.
        /// </summary>
        public static void BellLaserGridWarningVFX(Vector2 startPos, Vector2 direction, float length)
        {
            if (Main.dedServ) return;

            int segmentCount = (int)(length / 40f);
            for (int i = 0; i < segmentCount; i++)
            {
                float t = (float)i / segmentCount;
                Vector2 pos = startPos + direction * (length * t);

                if (Main.rand.NextBool(3))
                {
                    Color col = Color.Lerp(LaCampanellaPalette.DeepEmber, LaCampanellaPalette.InfernalOrange, t);
                    Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(5f, 5f),
                        DustID.Torch, Vector2.Zero, 0, col, 0.6f);
                    d.noGravity = true;
                }
            }
        }

        /// <summary>
        /// Bell Laser Grid fire — laser beam active VFX.
        /// </summary>
        public static void BellLaserGridFireVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Torch, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Bell Laser Grid intersection — fire explosion at laser crossings.
        /// </summary>
        public static void BellLaserIntersectionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.BellShockwaveImpact(pos, 0.6f);
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 4, 0.7f, 3f, 40);
        }

        // =====================================================================
        //  PHASE 3 ATTACKS
        // =====================================================================

        /// <summary>
        /// Triple Slam windup — triple-target warning circles.
        /// </summary>
        public static void TripleSlamWindupVFX(Vector2 targetPos, int slamIndex, float progress)
        {
            if (Main.dedServ) return;

            // Color intensity increases with each slam
            float intensity = 0.6f + slamIndex * 0.2f;

            int pointCount = 6 + (int)(progress * 6);
            for (int i = 0; i < pointCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pointCount;
                float radius = 35f * (1f - progress * 0.3f);
                Vector2 ringPos = targetPos + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(2))
                {
                    Color col = Color.Lerp(LaCampanellaPalette.DeepEmber, LaCampanellaPalette.WhiteHot, progress * intensity);
                    Dust d = Dust.NewDustPerfect(ringPos, DustID.Torch, Vector2.Zero, 0, col, 0.7f + progress * 0.4f);
                    d.noGravity = true;
                }
            }
        }

        /// <summary>
        /// Triple Slam impact — each individual slam hit.
        /// </summary>
        public static void TripleSlamImpactVFX(Vector2 pos, int slamIndex)
        {
            if (Main.dedServ) return;

            float intensity = 0.8f + slamIndex * 0.3f;

            LaCampanellaVFXLibrary.BellShockwaveImpact(pos, intensity);
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 4 + slamIndex * 2, 0.7f + slamIndex * 0.2f, 3f + slamIndex, 45);
            LaCampanellaVFXLibrary.SpawnEmberScatter(pos, 6 + slamIndex * 3, 4f + slamIndex);
            CustomParticles.LaCampanellaBellChime(pos, 8 + slamIndex * 3);

            MagnumScreenEffects.AddScreenShake(5f + slamIndex * 2f);
            Lighting.AddLight(pos, LaCampanellaPalette.WhiteHot.ToVector3() * (1.0f + slamIndex * 0.3f));
        }

        /// <summary>
        /// Infernal Torrent charge — energy gathering before barrage.
        /// </summary>
        public static void InfernalTorrentChargeVFX(Vector2 bossCenter, float progress)
        {
            if (Main.dedServ) return;

            // Radial converging particles
            int pointCount = 12;
            for (int i = 0; i < pointCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pointCount + progress * MathHelper.TwoPi;
                float radius = 60f * (1f - progress * 0.4f);
                Vector2 pos = bossCenter + angle.ToRotationVector2() * radius;

                Vector2 toCenter = (bossCenter - pos).SafeNormalize(Vector2.Zero) * (2f + progress * 4f);
                Color col = LaCampanellaPalette.GetFireGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, toCenter, 0, col, 1.0f + progress * 0.6f);
                d.noGravity = true;
            }

            Lighting.AddLight(bossCenter, LaCampanellaPalette.InfernalOrange.ToVector3() * (0.5f + progress * 0.5f));
        }

        /// <summary>
        /// Infernal Torrent barrage VFX — rapid projectile release pulse.
        /// </summary>
        public static void InfernalTorrentBarrageVFX(Vector2 pos, Vector2 direction)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(3f, 6f) + Main.rand.NextVector2Circular(1f, 1f);
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Inferno Cage warning — cage boundary outline.
        /// </summary>
        public static void InfernoCageWarningVFX(Vector2 cornerPos, float progress)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);
                Color col = Color.Lerp(LaCampanellaPalette.DeepEmber, LaCampanellaPalette.FlameYellow, progress);
                Dust d = Dust.NewDustPerfect(cornerPos + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.Torch, vel, 0, col, 0.8f + progress * 0.4f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Inferno Cage close VFX — walls closing in with fire particles.
        /// </summary>
        public static void InfernoCageCloseVFX(Vector2 wallPos)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.5f, 2f));
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(wallPos + Main.rand.NextVector2Circular(5f, 10f),
                    DustID.Torch, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            Lighting.AddLight(wallPos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Inferno Cage explosion — cage collapse fire eruption.
        /// </summary>
        public static void InfernoCageExplosionVFX(Vector2 center)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.BellShockwaveImpact(center, 1.0f);

            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                Color col = LaCampanellaPalette.GetFireGradient((float)i / 20f);
                Dust d = Dust.NewDustPerfect(center, DustID.Torch, vel, 0, col, 1.5f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.SpawnHeavySmoke(center, 8, 1.0f, 4f, 55);
            LaCampanellaVFXLibrary.SpawnEmberScatter(center, 10, 5f);

            MagnumScreenEffects.AddScreenShake(6f);
            Lighting.AddLight(center, LaCampanellaPalette.WhiteHot.ToVector3() * 1.3f);
        }

        /// <summary>
        /// Resonant Shock buildup — electrical fire arc buildup.
        /// </summary>
        public static void ResonantShockBuildupVFX(Vector2 bossCenter, float progress)
        {
            if (Main.dedServ) return;

            // Pulsing fire ring
            int pointCount = 16;
            float pulseFactor = 1f + MathF.Sin(progress * MathHelper.TwoPi * 3f) * 0.15f;
            for (int i = 0; i < pointCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pointCount;
                float radius = (40f + progress * 30f) * pulseFactor;
                Vector2 pos = bossCenter + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(2))
                {
                    Color col = Color.Lerp(LaCampanellaPalette.FlameYellow, LaCampanellaPalette.WhiteHot, progress);
                    Dust d = Dust.NewDustPerfect(pos, DustID.Torch, Vector2.Zero, 0, col, 0.9f + progress * 0.4f);
                    d.noGravity = true;
                }
            }

            // Ember scatter
            if (progress > 0.5f)
                LaCampanellaVFXLibrary.SpawnEmberScatter(bossCenter, (int)(progress * 4), 3f);

            Lighting.AddLight(bossCenter, LaCampanellaPalette.FlameYellow.ToVector3() * (0.5f + progress * 0.5f));
        }

        /// <summary>
        /// Resonant Shock release — chain lightning shockwave.
        /// </summary>
        public static void ResonantShockReleaseVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.BellShockwaveImpact(pos, 1.5f);

            for (int i = 0; i < 28; i++)
            {
                float angle = MathHelper.TwoPi * i / 28f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(7f, 12f);
                Color col = Color.Lerp(LaCampanellaPalette.FlameYellow, LaCampanellaPalette.WhiteHot,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.7f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 10, 1.1f, 5f, 65);
            CustomParticles.LaCampanellaBellChime(pos, 14);
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 6, 35f, 0.8f, 1.2f, 35);

            MagnumScreenEffects.AddScreenShake(8f);
            Lighting.AddLight(pos, LaCampanellaPalette.WhiteHot.ToVector3() * 2f);
        }

        /// <summary>
        /// Grand Finale windup — ultimate attack charge-up.
        /// </summary>
        public static void GrandFinaleWindupVFX(Vector2 bossCenter, float progress)
        {
            if (Main.dedServ) return;

            // Massive converging fire
            int dustCount = 8 + (int)(progress * 16);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 100f * (1f - progress * 0.6f) + Main.rand.NextFloat(20f);
                Vector2 dustPos = bossCenter + angle.ToRotationVector2() * radius;
                Vector2 toCenter = (bossCenter - dustPos).SafeNormalize(Vector2.Zero) * (3f + progress * 5f);
                Color col = Color.Lerp(LaCampanellaPalette.DeepEmber, LaCampanellaPalette.WhiteHot, progress);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, toCenter, 0, col, 1.2f + progress * 0.8f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.SpawnHeavySmoke(bossCenter, 3 + (int)(progress * 5), 0.6f + progress * 0.6f, 3f, 50);

            if (progress > 0.3f)
                LaCampanellaVFXLibrary.SpawnMusicNotes(bossCenter, 1 + (int)(progress * 3), 15f + progress * 20f, 0.7f, 1.0f + progress * 0.3f, 30);

            MagnumScreenEffects.AddScreenShake(2f + progress * 4f);
            Lighting.AddLight(bossCenter, LaCampanellaPalette.InfernalOrange.ToVector3() * (0.6f + progress * 0.8f));
        }

        /// <summary>
        /// Grand Finale slam — individual slam impacts in the multi-slam finale.
        /// </summary>
        public static void GrandFinaleSlamVFX(Vector2 pos, int slamPhase)
        {
            if (Main.dedServ) return;

            float intensity = 1.0f + slamPhase * 0.4f;
            LaCampanellaVFXLibrary.BellShockwaveImpact(pos, intensity);

            int dustCount = 16 + slamPhase * 4;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f + slamPhase * 2f, 9f + slamPhase * 2f);
                Color col = LaCampanellaPalette.GetFireGradient((float)i / dustCount);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.5f + slamPhase * 0.2f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 5 + slamPhase * 2, 0.8f + slamPhase * 0.2f, 4f, 50);
            CustomParticles.LaCampanellaBellChime(pos, 10 + slamPhase * 3);

            MagnumScreenEffects.AddScreenShake(6f + slamPhase * 2f);
            Lighting.AddLight(pos, LaCampanellaPalette.WhiteHot.ToVector3() * (1.2f + slamPhase * 0.4f));
        }

        /// <summary>
        /// Grand Finale final burst — the ultimate eruption ending the attack.
        /// </summary>
        public static void GrandFinaleBurstVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.InfernalEruption(pos, 2.0f);

            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 14f);
                Color col = LaCampanellaPalette.GetFireGradient((float)i / 40f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 2.0f);
                d.noGravity = true;
            }

            for (int i = 0; i < 4; i++)
            {
                CustomParticles.LaCampanellaBellChime(pos, 15 + i * 5);
            }

            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 14, 1.3f, 6f, 80);
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 10, 50f, 0.8f, 1.3f, 45);

            MagnumScreenEffects.AddScreenShake(12f);
            Lighting.AddLight(pos, LaCampanellaPalette.WhiteHot.ToVector3() * 3f);
        }

        // =====================================================================
        //  BOSS PROJECTILE VFX
        // =====================================================================

        /// <summary>
        /// InfernalBellLaser trail — fast beam with orange-gold fire trail.
        /// </summary>
        public static void InfernalBellLaserTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2.5f)
                + Main.rand.NextVector2Circular(0.3f, 0.3f);
            Color col = Color.Lerp(LaCampanellaPalette.InfernalOrange, LaCampanellaPalette.FlameYellow,
                Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.1f);
            d.noGravity = true;

            Lighting.AddLight(pos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.5f);
        }

        /// <summary>
        /// InfernalBellLaser impact — fire burst on death.
        /// </summary>
        public static void InfernalBellLaserImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;
            LaCampanellaVFXLibrary.ProjectileImpact(pos, 0.5f);
        }

        /// <summary>
        /// ExplosiveBellProjectile trail — arcing fire comet with sparkles.
        /// </summary>
        public static void ExplosiveBellTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f)
                + Main.rand.NextVector2Circular(0.3f, 0.3f);
            Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.0f);
            d.noGravity = true;

            // Golden sparkle accent
            if (Main.rand.NextBool(3))
            {
                Dust sparkle = Dust.NewDustPerfect(pos, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(0.5f, 0.5f), 0, LaCampanellaPalette.BellGold, 0.6f);
                sparkle.noGravity = true;
            }

            Lighting.AddLight(pos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.4f);
        }

        /// <summary>
        /// ExplosiveBellProjectile impact — bell explosion on contact.
        /// </summary>
        public static void ExplosiveBellImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.BellShockwaveImpact(pos, 0.7f);

            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color col = LaCampanellaPalette.GetFireGradient((float)i / 10f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 4, 0.7f, 3f, 40);
            CustomParticles.LaCampanellaBellChime(pos, 6);

            MagnumScreenEffects.AddScreenShake(3f);
            Lighting.AddLight(pos, LaCampanellaPalette.FlameYellow.ToVector3() * 0.9f);
        }

        /// <summary>
        /// InfernalGroundFire active burn — lingering fire hazard per-frame VFX.
        /// </summary>
        public static void InfernalGroundFireBurnVFX(Vector2 pos, float fadeProgress)
        {
            if (Main.dedServ) return;

            float alpha = 1f - fadeProgress;
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f - Main.rand.NextFloat(1f));
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + new Vector2(Main.rand.NextFloat(-12f, 12f), 0),
                    DustID.Torch, vel, 0, col, 1.0f * alpha);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.5f * alpha);
        }

        /// <summary>
        /// InfernalFireWave trail — ground-traveling fire wave per-frame VFX.
        /// </summary>
        public static void InfernalFireWaveTrailVFX(Vector2 pos, Vector2 velocity, float scale)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 2; i++)
            {
                Vector2 spawnPos = pos + new Vector2(Main.rand.NextFloat(-20f * scale, 20f * scale),
                    Main.rand.NextFloat(-30f * scale, 0));
                Vector2 vel = new Vector2(velocity.X * 0.3f, -1.5f);
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.Torch, vel, 0, col, 1.2f * scale);
                d.noGravity = true;
            }

            if (Main.rand.NextBool(3))
                LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 1, 0.5f * scale, 1.5f, 30);

            Lighting.AddLight(pos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.8f);
        }

        /// <summary>
        /// MassiveInfernalLaser beam — per-frame VFX along beam length.
        /// </summary>
        public static void MassiveInfernalLaserBeamVFX(Vector2 beamPos)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(beamPos + Main.rand.NextVector2Circular(15f, 15f),
                    DustID.Torch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            Lighting.AddLight(beamPos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.9f);
        }

        /// <summary>
        /// MassiveInfernalLaser end point — fire eruption at beam terminus.
        /// </summary>
        public static void MassiveInfernalLaserEndVFX(Vector2 endPos)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(endPos, DustID.Torch, vel, 0, col, 1.5f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.SpawnEmberScatter(endPos, 3, 3f);
            Lighting.AddLight(endPos, LaCampanellaPalette.FlameYellow.ToVector3() * 1.0f);
        }

        /// <summary>
        /// MassiveInfernalLaser source — fire buildup at beam origin.
        /// </summary>
        public static void MassiveInfernalLaserSourceVFX(Vector2 sourcePos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.DrawBloom(sourcePos, 0.5f);
            LaCampanellaVFXLibrary.SpawnEmberScatter(sourcePos, 2, 2f);

            Lighting.AddLight(sourcePos, LaCampanellaPalette.WhiteHot.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  BOSS GROUNDED STATE
        // =====================================================================

        /// <summary>
        /// Boss grounded fire — constant fire effects while boss is on the ground.
        /// </summary>
        public static void BossGroundedFireVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(2))
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -1f - Main.rand.NextFloat(1f));
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(20f, 10f),
                    DustID.Torch, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.SpawnAmbientSmoke(pos, 30f);
            Lighting.AddLight(pos, LaCampanellaPalette.InfernalOrange.ToVector3() * 0.5f);
        }
    }

    // #########################################################################
    //  LEGACY COMPATIBILITY WRAPPER
    // #########################################################################

    /// <summary>
    /// Legacy shared VFX helper — delegates to CrawlerOfTheBellVFX and ChimeOfLifeVFX.
    /// Maintained for backward compatibility with existing call sites.
    /// </summary>
    public static class LaCampanellaEnemyVFX
    {
        // Crawler delegates
        public static void CrawlerAmbientAura(NPC npc, float auraIntensity) =>
            CrawlerOfTheBellVFX.AmbientAura(npc, auraIntensity);

        public static void CrawlerAlertBurst(Vector2 pos) =>
            CrawlerOfTheBellVFX.AlertBurst(pos);

        public static void BellTollFlamesWindupVFX(Vector2 castPos, float progress) =>
            CrawlerOfTheBellVFX.BellTollFlamesWindupVFX(castPos, progress);

        public static void BellTollFlamesBurstVFX(Vector2 pos) =>
            CrawlerOfTheBellVFX.BellTollFlamesBurstVFX(pos);

        public static void InfernalCrawlTrailVFX(NPC npc) =>
            CrawlerOfTheBellVFX.InfernalCrawlTrailVFX(npc);

        public static void SmokeBurstVFX(Vector2 pos) =>
            CrawlerOfTheBellVFX.SmokeBurstVFX(pos);

        public static void ChimePillarWindupVFX(Vector2 pillarPos, float progress) =>
            CrawlerOfTheBellVFX.ChimePillarWindupVFX(pillarPos, progress);

        public static void ChimePillarEruptionVFX(Vector2 pillarPos) =>
            CrawlerOfTheBellVFX.ChimePillarEruptionVFX(pillarPos);

        public static void CrescendoInfernoWarningVFX(Vector2 pos, float warningRadius, float progress) =>
            CrawlerOfTheBellVFX.CrescendoInfernoWarningVFX(pos, warningRadius, progress);

        public static void CrescendoInfernoImpactVFX(Vector2 pos) =>
            CrawlerOfTheBellVFX.CrescendoInfernoImpactVFX(pos);

        public static void EnemyHitVFX(NPC npc) =>
            CrawlerOfTheBellVFX.HitVFX(npc);

        public static void EnemyDeathVFX(NPC npc) =>
            CrawlerOfTheBellVFX.DeathVFX(npc);

        public static void EnemyProjectileTrailVFX(Vector2 pos, Vector2 velocity) =>
            CrawlerOfTheBellVFX.BellTollFlameTrailVFX(pos, velocity);

        public static void EnemyProjectileImpactVFX(Vector2 pos) =>
            CrawlerOfTheBellVFX.BellTollFlameImpactVFX(pos);

        // Boss delegates
        public static void BossAmbientAura(NPC npc, float auraIntensity) =>
            ChimeOfLifeVFX.BossAmbientAura(npc, auraIntensity);

        public static void BossPhaseTransitionVFX(Vector2 pos) =>
            ChimeOfLifeVFX.BossPhaseTransitionVFX(pos, 2);

        public static void BellLaserIntersectionVFX(Vector2 pos) =>
            ChimeOfLifeVFX.BellLaserIntersectionVFX(pos);
    }
}
