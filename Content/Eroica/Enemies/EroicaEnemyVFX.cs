using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Screen;
using static MagnumOpus.Common.Systems.VFX.GodRaySystem;

namespace MagnumOpus.Content.Eroica.Enemies
{
    /// <summary>
    /// Unified VFX system for ALL Eroica-themed enemies. Each enemy type has a
    /// distinct visual personality while sharing the corrupted-heroic aesthetic:
    ///
    ///   Behemoth of Valor  -- Heavy war drums, ground-shaking slams, massive ember cascades
    ///   Eroican Centurion  -- Military precision, disciplined formations, clean sparks
    ///   Funeral Blitzer    -- Rapid-fire mourning barrage, funeral smoke, staccato notes
    ///   Stolen Valor       -- Corrupted commander, 5 minions, false glory, stolen fire
    ///   StolenValorMinion  -- Corrupted flame echoes orbiting the commander
    ///
    /// All VFX route through EroicaVFXLibrary for canonical palette + modern systems.
    /// Game logic (AI, projectile spawning, debuffs) stays in the individual enemy files.
    /// </summary>
    public static class EroicaEnemyVFX
    {
        // ======================================================================
        //  BEHEMOTH OF VALOR — Accent Palette
        //  Heavy battle aura, shaking ground, war drums
        // ======================================================================

        private static readonly Color BehemothScarlet = new Color(190, 45, 40);
        private static readonly Color WarGold = new Color(240, 200, 60);
        private static readonly Color HeavyCore = new Color(200, 80, 50);

        // ======================================================================
        //  EROICAN CENTURION — Accent Palette
        //  Military precision, disciplined flame, organised formations
        // ======================================================================

        private static readonly Color CenturionRed = new Color(200, 55, 45);
        private static readonly Color LegionGold = new Color(255, 210, 70);
        private static readonly Color ShieldBronze = new Color(180, 140, 80);

        // ======================================================================
        //  FUNERAL BLITZER — Accent Palette
        //  Rapid-fire funeral projectiles, mourning barrage, black smoke trails
        // ======================================================================

        private static readonly Color BlitzerScarlet = new Color(210, 40, 35);
        private static readonly Color FuneralSmoke = new Color(60, 40, 50);
        private static readonly Color BlitzGold = new Color(240, 180, 60);
        private static readonly Color RapidFlame = new Color(255, 100, 60);

        // ======================================================================
        //  STOLEN VALOR — Accent Palette
        //  Corrupted commander, stolen heroic fire, false glory
        // ======================================================================

        private static readonly Color StolenScarlet = new Color(180, 50, 50);
        private static readonly Color CorruptGold = new Color(200, 180, 60);
        private static readonly Color CommanderCrimson = new Color(160, 35, 40);
        private static readonly Color FalseGlory = new Color(220, 200, 100);
        private static readonly Color MinionFlame = new Color(200, 70, 50);

        // ======================================================================
        //  BEHEMOTH OF VALOR — VFX Methods
        // ======================================================================

        #region Behemoth of Valor

        /// <summary>
        /// Ambient aura for the Behemoth: heavy ember trail, rising war-flame motes,
        /// periodic deep music notes, ground-shake dust, and intense scarlet lighting.
        /// </summary>
        public static void BehemothAmbientAura(Vector2 center, int frameCounter)
        {
            // Heavy ember trail — 3 large dust particles every 4 frames
            if (frameCounter % 4 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(40f, 30f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -Main.rand.NextFloat(1f, 2.5f));
                    Color col = Color.Lerp(BehemothScarlet, WarGold, Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(center + offset, DustID.Torch, vel, 0, col, 1.8f);
                    d.noGravity = true;
                }

                // Rising war-flame motes
                Vector2 motePos = center + new Vector2(Main.rand.NextFloat(-50f, 50f), 20f);
                Dust mote = Dust.NewDustPerfect(motePos, DustID.GoldFlame,
                    new Vector2(0f, -Main.rand.NextFloat(0.8f, 1.8f)), 0, WarGold, 1.4f);
                mote.noGravity = true;
            }

            // Deep heroic music note every 15 frames
            if (frameCounter % 15 == 0)
            {
                EroicaVFXLibrary.SpawnMusicNotes(center, 1, 30f, 0.9f, 1.1f, 40);
            }

            // Ground-shake dust below feet
            if (frameCounter % 6 == 0)
            {
                Vector2 groundPos = center + new Vector2(Main.rand.NextFloat(-60f, 60f), 50f);
                Dust ground = Dust.NewDustPerfect(groundPos, DustID.Smoke,
                    new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(0.5f, 1.5f)),
                    150, Color.Gray, 1.2f);
                ground.noGravity = false;
            }

            Lighting.AddLight(center, BehemothScarlet.ToVector3() * 0.9f);
        }

        /// <summary>
        /// Heavy slam attack: ground impact bloom, shockwave ring, directional ember spray,
        /// war drum dust burst, screen shake, and a music note chord.
        /// </summary>
        public static void BehemothAttackVFX(Vector2 attackPos, Vector2 direction)
        {
            // Ground impact bloom
            EroicaVFXLibrary.BloomFlare(attackPos, BehemothScarlet, 0.9f, 20);
            EroicaVFXLibrary.SpawnGradientHaloRings(attackPos, 4, 0.5f);

            // Shockwave ring
            var shockRing = new BloomRingParticle(attackPos, Vector2.Zero,
                WarGold * 0.9f, 0.7f, 28, 0.14f);
            MagnumParticleHandler.SpawnParticle(shockRing);

            // Directional ember spray — 10 heavy particles
            for (int i = 0; i < 10; i++)
            {
                float spreadAngle = Main.rand.NextFloat(-0.6f, 0.6f);
                Vector2 vel = direction.RotatedBy(spreadAngle) * Main.rand.NextFloat(5f, 10f);
                Color col = Color.Lerp(BehemothScarlet, WarGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(attackPos, DustID.Torch, vel, 0, col, 2.0f);
                d.noGravity = true;
            }

            // War drum dust burst
            EroicaVFXLibrary.SpawnRadialDustBurst(attackPos, 15, 8f, DustID.GoldFlame);

            // Screen shake & sound
            MagnumScreenEffects.AddScreenShake(3f);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 0.7f }, attackPos);

            // Music note chord — heroic war theme
            EroicaVFXLibrary.SpawnMusicNotes(attackPos, 4, 35f, 0.8f, 1.1f, 35);

            Lighting.AddLight(attackPos, HeavyCore.ToVector3() * 1.4f);
        }

        /// <summary>
        /// Death VFX: massive DeathHeroicFlash, ember scatter, war-gold halo cascade,
        /// music note burst, and god rays.
        /// </summary>
        public static void BehemothDeathVFX(Vector2 center)
        {
            EroicaVFXLibrary.DeathHeroicFlash(center, 1.2f);

            // Massive ember scatter
            for (int i = 0; i < 30; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(14f, 14f);
                Color col = Color.Lerp(BehemothScarlet, WarGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(center, DustID.Torch, vel, 0, col, 2.5f);
                d.noGravity = true;
            }

            // War-gold halo cascade
            for (int i = 0; i < 6; i++)
            {
                float progress = (float)i / 6;
                Color ringCol = Color.Lerp(BehemothScarlet, WarGold, progress);
                var ring = new BloomRingParticle(center, Vector2.Zero, ringCol,
                    0.4f + i * 0.15f, 30, 0.10f + i * 0.02f);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // Music note burst — 8 radial heroic
            EroicaVFXLibrary.MusicNoteBurst(center, WarGold, 8, 6f);

            // God rays
            GodRaySystem.CreateBurst(center, BehemothScarlet, 8, 120f, 40, GodRayStyle.Explosion, WarGold);

            Lighting.AddLight(center, EroicaPalette.HotCore.ToVector3() * 2.0f);
        }

        #endregion

        // ======================================================================
        //  EROICAN CENTURION — VFX Methods
        // ======================================================================

        #region Eroican Centurion

        /// <summary>
        /// Ambient aura for the Centurion: disciplined 2-point orbit, military precision
        /// sparkles, and periodic military march music notes.
        /// </summary>
        public static void CenturionAmbientAura(Vector2 center, int frameCounter)
        {
            // Disciplined 2-point orbit — tight radius, CenturionRed
            if (frameCounter % 8 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    float angle = frameCounter * 0.08f + i * MathHelper.Pi;
                    Vector2 orbitPos = center + angle.ToRotationVector2() * 20f;
                    Dust orbit = Dust.NewDustPerfect(orbitPos, DustID.Torch,
                        Vector2.Zero, 0, CenturionRed, 1.3f);
                    orbit.noGravity = true;
                }

                // Military precision sparkles — clean, sharp
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(15f, 15f);
                Dust spark = Dust.NewDustPerfect(sparkPos, DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(0.5f, 0.5f), 0, LegionGold, 0.9f);
                spark.noGravity = true;
            }

            // Military march music note every 20 frames
            if (frameCounter % 20 == 0)
            {
                EroicaVFXLibrary.SpawnMusicNotes(center, 1, 15f, 0.7f, 0.9f, 30);
            }

            Lighting.AddLight(center, CenturionRed.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Disciplined sword swing: clean directional spark line, small bloom,
        /// small halo ring, military-precision aesthetics.
        /// </summary>
        public static void CenturionSwordSwingVFX(Vector2 swingPos, Vector2 direction)
        {
            // Clean directional spark line
            for (int i = 0; i < 6; i++)
            {
                float t = i / 6f;
                Vector2 sparkPos = swingPos + direction * (t * 30f);
                Dust s = Dust.NewDustPerfect(sparkPos, DustID.Enchanted_Gold,
                    direction * Main.rand.NextFloat(2f, 5f), 0, LegionGold, 1.0f);
                s.noGravity = true;
            }

            // Small bloom at impact
            EroicaVFXLibrary.BloomFlare(swingPos, CenturionRed, 0.45f, 12);

            // Small halo
            var halo = new BloomRingParticle(swingPos, Vector2.Zero,
                LegionGold * 0.7f, 0.25f, 18, 0.05f);
            MagnumParticleHandler.SpawnParticle(halo);

            Lighting.AddLight(swingPos, LegionGold.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Lantern ambient effect: warm golden glow, gentle pulsing, flame motes
        /// rising from the lantern, occasional music note.
        /// </summary>
        public static void CenturionLanternVFX(Vector2 lanternPos, int frameCounter)
        {
            // Warm golden glow — LegionGold pulsing
            float pulse = (float)Math.Sin(frameCounter * 0.08f) * 0.15f + 0.85f;

            if (frameCounter % 5 == 0)
            {
                // Flame motes rising from lantern
                Dust flame = Dust.NewDustPerfect(
                    lanternPos + Main.rand.NextVector2Circular(6f, 4f),
                    DustID.Torch,
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.6f, 1.4f)),
                    0, LegionGold, Main.rand.NextFloat(0.8f, 1.2f));
                flame.noGravity = true;
            }

            // Occasional music note
            if (frameCounter % 30 == 0)
            {
                EroicaVFXLibrary.SpawnMusicNotes(lanternPos, 1, 8f, 0.6f, 0.8f, 25);
            }

            Lighting.AddLight(lanternPos, LegionGold.ToVector3() * 0.8f * pulse);
        }

        /// <summary>
        /// Death: military funeral flash, organised dust scatter, golden ember rain,
        /// somber music notes.
        /// </summary>
        public static void CenturionDeathVFX(Vector2 center)
        {
            EroicaVFXLibrary.DeathHeroicFlash(center, 0.8f);

            // Organised dust scatter — evenly spaced radial
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Dust d = Dust.NewDustPerfect(center, DustID.Torch, vel, 0, CenturionRed, 1.6f);
                d.noGravity = true;
            }

            // Golden ember rain — falling downward with spread
            for (int i = 0; i < 10; i++)
            {
                Vector2 pos = center + new Vector2(Main.rand.NextFloat(-40f, 40f), -Main.rand.NextFloat(10f, 40f));
                Dust gold = Dust.NewDustPerfect(pos, DustID.GoldFlame,
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(0.5f, 2f)),
                    0, LegionGold, 1.3f);
                gold.noGravity = true;
            }

            // Somber music notes — slower, lower
            EroicaVFXLibrary.SpawnMusicNotes(center, 5, 30f, 0.7f, 0.9f, 45);

            Lighting.AddLight(center, LegionGold.ToVector3() * 1.2f);
        }

        #endregion

        // ======================================================================
        //  FUNERAL BLITZER — VFX Methods
        // ======================================================================

        #region Funeral Blitzer

        /// <summary>
        /// Ambient aura: rapid flickering flame motes, trailing funeral smoke,
        /// quick staccato music notes.
        /// </summary>
        public static void BlitzerAmbientAura(Vector2 center, int frameCounter)
        {
            // Rapid flickering flame motes every 6 frames
            if (frameCounter % 6 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(18f, 14f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(0.8f, 2f));
                    Dust d = Dust.NewDustPerfect(center + offset, DustID.Torch, vel, 0,
                        Color.Lerp(BlitzerScarlet, RapidFlame, Main.rand.NextFloat()), 1.2f);
                    d.noGravity = true;
                }

                // Trailing funeral smoke
                Dust smoke = Dust.NewDustPerfect(
                    center + Main.rand.NextVector2Circular(12f, 8f),
                    DustID.Smoke,
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.3f, 0.8f)),
                    180, FuneralSmoke, 1.0f);
                smoke.noGravity = false;
            }

            // Quick staccato music note every 12 frames
            if (frameCounter % 12 == 0)
            {
                EroicaVFXLibrary.SpawnMusicNotes(center, 1, 12f, 0.6f, 0.8f, 20);
            }

            Lighting.AddLight(center, BlitzerScarlet.ToVector3() * 0.45f);
        }

        /// <summary>
        /// Rapid fire: quick muzzle flash, directional smoke puff, tracer dust,
        /// staccato music note (1/3 chance).
        /// </summary>
        public static void BlitzerFireVFX(Vector2 firePos, Vector2 direction)
        {
            // Quick muzzle flash — small bloom
            EroicaVFXLibrary.BloomFlare(firePos, RapidFlame, 0.35f, 8);

            // Directional smoke puff
            for (int i = 0; i < 3; i++)
            {
                Vector2 smokeVel = -direction * Main.rand.NextFloat(1f, 3f)
                    + Main.rand.NextVector2Circular(1f, 1f);
                Dust smoke = Dust.NewDustPerfect(firePos, DustID.Smoke, smokeVel,
                    160, FuneralSmoke, Main.rand.NextFloat(0.8f, 1.3f));
                smoke.noGravity = false;
            }

            // Tracer dust
            for (int i = 0; i < 3; i++)
            {
                Vector2 tracerVel = direction * Main.rand.NextFloat(4f, 8f)
                    + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Dust tracer = Dust.NewDustPerfect(firePos, DustID.Torch, tracerVel, 0,
                    BlitzerScarlet, 1.1f);
                tracer.noGravity = true;
            }

            // Staccato music note — 1/3 chance
            if (Main.rand.NextBool(3))
            {
                EroicaVFXLibrary.SpawnMusicNotes(firePos, 1, 8f, 0.5f, 0.7f, 18);
            }

            SoundEngine.PlaySound(SoundID.Item11 with { Pitch = 0.4f, Volume = 0.4f }, firePos);
            Lighting.AddLight(firePos, RapidFlame.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Per-frame projectile trail: mourning flame trail (BlitzerScarlet to BlitzGold),
        /// FuneralSmoke wisps, dim lighting.
        /// </summary>
        public static void BlitzerProjectileTrail(Projectile proj)
        {
            // Mourning flame trail
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = -proj.velocity * 0.15f + Main.rand.NextVector2Circular(0.4f, 0.4f);
                Color col = Color.Lerp(BlitzerScarlet, BlitzGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(proj.Center, DustID.Torch, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            // FuneralSmoke wisps
            if (Main.rand.NextBool(4))
            {
                Dust smoke = Dust.NewDustPerfect(
                    proj.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.Smoke,
                    -proj.velocity * 0.08f + new Vector2(0f, -0.3f),
                    140, FuneralSmoke, 0.7f);
                smoke.noGravity = false;
            }

            Lighting.AddLight(proj.Center, BlitzerScarlet.ToVector3() * 0.3f);
        }

        /// <summary>
        /// Small projectile impact: crimson dust burst, small bloom, smoke puff.
        /// </summary>
        public static void BlitzerProjectileHitVFX(Vector2 hitPos)
        {
            // Crimson dust burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Torch, vel, 0,
                    BlitzerScarlet, 1.2f);
                d.noGravity = true;
            }

            // Small bloom
            EroicaVFXLibrary.BloomFlare(hitPos, BlitzerScarlet, 0.3f, 10);

            // Smoke puff
            Dust smoke = Dust.NewDustPerfect(hitPos, DustID.Smoke,
                new Vector2(0f, -0.8f), 150, FuneralSmoke, 1.0f);
            smoke.noGravity = false;

            Lighting.AddLight(hitPos, BlitzerScarlet.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Death: funeral pyre collapse, smoke explosion, ember scatter, music notes.
        /// </summary>
        public static void BlitzerDeathVFX(Vector2 center)
        {
            EroicaVFXLibrary.DeathHeroicFlash(center, 0.7f);

            // Smoke explosion
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust smoke = Dust.NewDustPerfect(center, DustID.Smoke, vel,
                    180, FuneralSmoke, Main.rand.NextFloat(1.5f, 2.5f));
                smoke.noGravity = false;
            }

            // Ember scatter
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Color col = Color.Lerp(BlitzerScarlet, BlitzGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(center, DustID.Torch, vel, 0, col, 1.8f);
                d.noGravity = true;
            }

            // Music notes — funeral lament
            EroicaVFXLibrary.SpawnMusicNotes(center, 4, 25f, 0.7f, 0.9f, 35);

            Lighting.AddLight(center, BlitzGold.ToVector3() * 1.0f);
        }

        #endregion

        // ======================================================================
        //  STOLEN VALOR (Multi-Phase Mini-Boss) — VFX Methods
        // ======================================================================

        #region Stolen Valor

        /// <summary>
        /// Ambient aura: corrupted flame orbit (4 points), stolen valor sparkles,
        /// attack-state reactive intensity, corrupted music note.
        /// </summary>
        public static void StolenValorAmbientAura(Vector2 center, int frameCounter, int attackState)
        {
            float intensityMult = attackState > 0 ? 1.5f : 1.0f;

            // Corrupted flame orbit — 4 points, larger radius 35f
            if (frameCounter % 5 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = frameCounter * 0.04f + i * MathHelper.PiOver2;
                    Vector2 orbitPos = center + angle.ToRotationVector2() * 35f;
                    Color col = Color.Lerp(StolenScarlet, CorruptGold, Main.rand.NextFloat());
                    Dust orbit = Dust.NewDustPerfect(orbitPos, DustID.Torch,
                        Main.rand.NextVector2Circular(0.3f, 0.3f), 0, col, 1.4f * intensityMult);
                    orbit.noGravity = true;
                }

                // Stolen valor sparkles
                for (int i = 0; i < (int)(2 * intensityMult); i++)
                {
                    Vector2 sparkPos = center + Main.rand.NextVector2Circular(30f, 25f);
                    Dust spark = Dust.NewDustPerfect(sparkPos, DustID.GoldFlame,
                        Main.rand.NextVector2Circular(0.8f, 0.8f), 0, CorruptGold, 1.0f);
                    spark.noGravity = true;
                }
            }

            // Corrupted music note every 15 frames — slightly off-key feel
            if (frameCounter % 15 == 0)
            {
                EroicaVFXLibrary.SpawnMusicNotes(center, 1, 20f, 0.7f, 1.0f, 32);
            }

            Lighting.AddLight(center, StolenScarlet.ToVector3() * 0.6f * intensityMult);
        }

        /// <summary>
        /// Commander order: radial command pulse, connecting dust chains,
        /// bloom at command center, distorted music note burst.
        /// </summary>
        public static void CommanderOrderVFX(Vector2 commandCenter, int attackType)
        {
            // Radial command pulse — expanding CommanderCrimson ring
            var cmdRing = new BloomRingParticle(commandCenter, Vector2.Zero,
                CommanderCrimson * 0.9f, 0.6f, 25, 0.12f);
            MagnumParticleHandler.SpawnParticle(cmdRing);

            var innerRing = new BloomRingParticle(commandCenter, Vector2.Zero,
                CorruptGold * 0.7f, 0.35f, 20, 0.08f);
            MagnumParticleHandler.SpawnParticle(innerRing);

            // Bloom at command center
            EroicaVFXLibrary.BloomFlare(commandCenter, CommanderCrimson, 0.6f, 16);

            // Distorted music note burst — corrupted, commanding
            EroicaVFXLibrary.SpawnMusicNotes(commandCenter, 3, 25f, 0.8f, 1.1f, 30);

            // Command sparks radiating outward
            EroicaVFXLibrary.SpawnRadialDustBurst(commandCenter, 10, 6f, DustID.Torch);

            SoundEngine.PlaySound(SoundID.Item73 with { Pitch = -0.2f, Volume = 0.5f }, commandCenter);
            Lighting.AddLight(commandCenter, CommanderCrimson.ToVector3() * 1.0f);
        }

        /// <summary>
        /// All 5 minions fire simultaneously: rapid bloom pulses, directional sparks,
        /// staccato music.
        /// </summary>
        public static void MinionBarrageVFX(Vector2 center)
        {
            // Rapid bloom pulse at center
            EroicaVFXLibrary.BloomFlare(center, StolenScarlet, 0.5f, 12);
            EroicaVFXLibrary.SpawnGradientHaloRings(center, 3, 0.3f);

            // Directional sparks outward
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Dust d = Dust.NewDustPerfect(center, DustID.Torch, vel, 0, MinionFlame, 1.3f);
                d.noGravity = true;
            }

            // Staccato music notes
            EroicaVFXLibrary.SpawnMusicNotes(center, 2, 20f, 0.6f, 0.8f, 22);

            Lighting.AddLight(center, StolenScarlet.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Formation charge: directional ember trail, formation dust lines,
        /// war music notes.
        /// </summary>
        public static void ChargingFormationVFX(Vector2 center, Vector2 direction)
        {
            // Directional ember trail
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = -direction * Main.rand.NextFloat(2f, 5f)
                    + Main.rand.NextVector2Circular(1f, 1f);
                Color col = Color.Lerp(StolenScarlet, CorruptGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(20f, 10f),
                    DustID.Torch, vel, 0, col, 1.5f);
                d.noGravity = true;
            }

            // Formation dust lines — parallel pair alongside charge direction
            Vector2 perp = new Vector2(-direction.Y, direction.X);
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 linePos = center + perp * side * 25f;
                Dust line = Dust.NewDustPerfect(linePos, DustID.GoldFlame,
                    direction * 2f, 0, CorruptGold, 1.0f);
                line.noGravity = true;
            }

            // War music notes
            EroicaVFXLibrary.SpawnMusicNotes(center, 2, 18f, 0.7f, 0.9f, 25);

            Lighting.AddLight(center, CorruptGold.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Spiral attack: expanding spiral dust trail, orbiting bloom nodes,
        /// cosmic music.
        /// </summary>
        public static void OrbitalBombardmentVFX(Vector2 center, float spiralAngle)
        {
            // Expanding spiral dust trail
            for (int i = 0; i < 5; i++)
            {
                float angle = spiralAngle + i * MathHelper.TwoPi / 5f;
                float radius = 30f + i * 15f;
                Vector2 spiralPos = center + angle.ToRotationVector2() * radius;
                Dust spiral = Dust.NewDustPerfect(spiralPos, DustID.Torch,
                    angle.ToRotationVector2() * 1.5f, 0, StolenScarlet, 1.3f);
                spiral.noGravity = true;
            }

            // Orbiting bloom nodes
            for (int i = 0; i < 3; i++)
            {
                float angle = spiralAngle * 0.5f + i * MathHelper.TwoPi / 3f;
                Vector2 nodePos = center + angle.ToRotationVector2() * 50f;
                EroicaVFXLibrary.BloomFlare(nodePos, CorruptGold, 0.25f, 8);
            }

            // Cosmic music
            if (Main.GameUpdateCount % 10 == 0)
            {
                EroicaVFXLibrary.SpawnMusicNotes(center, 1, 30f, 0.8f, 1.0f, 30);
            }

            Lighting.AddLight(center, StolenScarlet.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Cage formation: hexagonal dust cage around player, FalseGlory glow at each point,
        /// ominous music.
        /// </summary>
        public static void FalseGloryVFX(Vector2 center)
        {
            // Hexagonal dust cage — 6 points around center
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 cagePos = center + angle.ToRotationVector2() * 120f;

                // FalseGlory glow at each cage point
                Dust glow = Dust.NewDustPerfect(cagePos, DustID.GoldFlame,
                    Vector2.Zero, 0, FalseGlory, 1.6f);
                glow.noGravity = true;

                // Connecting dust between adjacent cage points
                float nextAngle = MathHelper.TwoPi * ((i + 1) % 6) / 6f;
                Vector2 nextPos = center + nextAngle.ToRotationVector2() * 120f;
                Vector2 mid = (cagePos + nextPos) * 0.5f;
                Dust link = Dust.NewDustPerfect(mid, DustID.Torch,
                    Vector2.Zero, 0, CommanderCrimson, 0.9f);
                link.noGravity = true;
            }

            // Ominous music
            EroicaVFXLibrary.SpawnMusicNotes(center, 2, 40f, 0.8f, 1.0f, 35);

            Lighting.AddLight(center, FalseGlory.ToVector3() * 0.7f);
        }

        /// <summary>
        /// ULTIMATE attack: massive explosion, stolen heroic flash, 6-point star burst,
        /// halo cascade (8 rings), music note storm (16 radial), screen shake (8f),
        /// screen ripple, god rays (8 rays).
        /// </summary>
        public static void StolenTriumphVFX(Vector2 center)
        {
            // Stolen heroic flash
            EroicaVFXLibrary.DeathHeroicFlash(center, 1.5f);

            // 6-point star burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 dir = angle.ToRotationVector2();
                for (int j = 0; j < 5; j++)
                {
                    Vector2 vel = dir * Main.rand.NextFloat(6f, 14f);
                    Color col = Color.Lerp(StolenScarlet, FalseGlory, Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(center, DustID.Torch, vel, 0, col, 2.2f);
                    d.noGravity = true;
                }
            }

            // Halo cascade — 8 expanding rings
            for (int i = 0; i < 8; i++)
            {
                float progress = (float)i / 8;
                Color ringCol = Color.Lerp(CommanderCrimson, FalseGlory, progress);
                var ring = new BloomRingParticle(center, Vector2.Zero, ringCol,
                    0.5f + i * 0.18f, 35, 0.10f + i * 0.025f);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // Music note storm — 16 radial notes
            EroicaVFXLibrary.MusicNoteBurst(center, CorruptGold, 16, 7f);

            // Screen shake
            MagnumScreenEffects.AddScreenShake(8f);

            // Screen ripple
            ScreenDistortionManager.TriggerRipple(center, StolenScarlet, 1.0f, 30);

            // God rays — 8 rays of stolen glory
            GodRaySystem.CreateBurst(center, CommanderCrimson, 8, 130f, 45, GodRayStyle.Explosion, FalseGlory);

            Lighting.AddLight(center, EroicaPalette.HotCore.ToVector3() * 2.5f);
        }

        /// <summary>
        /// Death: corrupted valor shatters, massive DeathHeroicFlash 1.5f, ember cascade,
        /// distorted music note burst, god rays, stolen fire dissipates into true gold
        /// via a colour transition effect.
        /// </summary>
        public static void StolenValorDeathVFX(Vector2 center)
        {
            // DeathHeroicFlash at 1.5x scale
            EroicaVFXLibrary.DeathHeroicFlash(center, 1.5f);

            // Massive ember cascade — corrupted colours transitioning to true gold
            for (int i = 0; i < 40; i++)
            {
                float progress = (float)i / 40;
                Vector2 vel = Main.rand.NextVector2Circular(16f, 16f);
                // Colour transition: StolenScarlet -> true EroicaPalette.Gold
                Color col = Color.Lerp(StolenScarlet, EroicaPalette.Gold, progress);
                Dust d = Dust.NewDustPerfect(center, DustID.Torch, vel, 0, col, 2.0f);
                d.noGravity = true;
            }

            // True gold dust ring — the stolen fire returns to honest gold
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f, 10f);
                Dust gold = Dust.NewDustPerfect(center, DustID.GoldFlame, vel, 0,
                    EroicaPalette.Gold, 2.2f);
                gold.noGravity = true;
            }

            // Distorted music note burst
            EroicaVFXLibrary.MusicNoteBurst(center, CorruptGold, 10, 6f);

            // God rays
            GodRaySystem.CreateBurst(center, StolenScarlet, 8, 110f, 40, GodRayStyle.Explosion, EroicaPalette.Gold);

            // Screen shake
            MagnumScreenEffects.AddScreenShake(6f);
            ScreenDistortionManager.TriggerRipple(center, CommanderCrimson, 0.8f, 25);

            Lighting.AddLight(center, EroicaPalette.HotCore.ToVector3() * 2.0f);
        }

        #endregion

        // ======================================================================
        //  STOLEN VALOR MINION — VFX Methods
        // ======================================================================

        #region StolenValor Minion

        /// <summary>
        /// Subtle corrupted flame glow, orbiting mote, ambient lighting.
        /// </summary>
        public static void MinionAmbientGlow(Vector2 center, int frameCounter)
        {
            // Corrupted flame glow — subtle pulsing
            if (frameCounter % 8 == 0)
            {
                Dust glow = Dust.NewDustPerfect(
                    center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Torch,
                    new Vector2(0f, -Main.rand.NextFloat(0.3f, 0.8f)),
                    0, MinionFlame, 1.0f);
                glow.noGravity = true;
            }

            // Orbiting mote — single point
            if (frameCounter % 12 == 0)
            {
                float angle = frameCounter * 0.1f;
                Vector2 motePos = center + angle.ToRotationVector2() * 12f;
                Dust mote = Dust.NewDustPerfect(motePos, DustID.GoldFlame,
                    Vector2.Zero, 0, CorruptGold, 0.7f);
                mote.noGravity = true;
            }

            Lighting.AddLight(center, MinionFlame.ToVector3() * 0.35f);
        }

        /// <summary>
        /// Attack: corrupted flame burst, dark tracer.
        /// </summary>
        public static void MinionFireVFX(Vector2 firePos, Vector2 direction)
        {
            // Corrupted flame burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(3f, 7f)
                    + Main.rand.NextVector2Circular(1f, 1f);
                Dust d = Dust.NewDustPerfect(firePos, DustID.Torch, vel, 0,
                    MinionFlame, 1.2f);
                d.noGravity = true;
            }

            // Dark tracer
            Dust tracer = Dust.NewDustPerfect(firePos, DustID.Smoke,
                direction * 3f, 120, StolenScarlet, 0.8f);
            tracer.noGravity = true;

            EroicaVFXLibrary.BloomFlare(firePos, MinionFlame, 0.25f, 8);
            Lighting.AddLight(firePos, MinionFlame.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Small death: corrupted flash, ember scatter.
        /// </summary>
        public static void MinionDeathVFX(Vector2 center)
        {
            EroicaVFXLibrary.BloomFlare(center, StolenScarlet, 0.4f, 12);

            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Color col = Color.Lerp(MinionFlame, CorruptGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(center, DustID.Torch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            EroicaVFXLibrary.SpawnMusicNotes(center, 1, 10f, 0.5f, 0.7f, 20);
            Lighting.AddLight(center, MinionFlame.ToVector3() * 0.6f);
        }

        #endregion

        // ======================================================================
        //  SHARED ENEMY HELPERS
        // ======================================================================

        #region Shared Helpers

        /// <summary>
        /// Generic spawn burst for any Eroica enemy. Radial dust, bloom flash,
        /// halo rings, and a music note flourish.
        /// </summary>
        public static void EnemySpawnBurst(Vector2 pos, Color primaryColor)
        {
            // Bloom flash
            EroicaVFXLibrary.BloomFlare(pos, primaryColor, 0.6f, 18);

            // Halo rings in primary colour
            for (int i = 0; i < 3; i++)
            {
                float progress = (float)i / 3;
                Color ringCol = Color.Lerp(primaryColor, EroicaPalette.Gold, progress);
                var ring = new BloomRingParticle(pos, Vector2.Zero, ringCol,
                    0.3f + i * 0.1f, 20, 0.06f + i * 0.02f);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // Radial dust burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color col = Color.Lerp(primaryColor, EroicaPalette.Gold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.5f);
                d.noGravity = true;
            }

            // Music note flourish
            EroicaVFXLibrary.SpawnMusicNotes(pos, 3, 20f, 0.6f, 0.9f, 28);

            SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.4f }, pos);
            Lighting.AddLight(pos, primaryColor.ToVector3() * 1.0f);
        }

        /// <summary>
        /// On-hit reaction flash for any Eroica enemy. Small bloom, directional
        /// sparks in the accent colour, intensity-scaled lighting.
        /// </summary>
        public static void EnemyHitFlash(Vector2 hitPos, Color accentColor, float intensity)
        {
            // Small bloom flash
            EroicaVFXLibrary.BloomFlare(hitPos, accentColor, 0.3f * intensity, 10);

            // Directional sparks
            for (int i = 0; i < (int)(4 * intensity); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f) * intensity;
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Torch, vel, 0, accentColor, 1.1f);
                d.noGravity = true;
            }

            // Music sparkle (50% chance)
            if (Main.rand.NextBool(2))
            {
                Dust sparkle = Dust.NewDustPerfect(hitPos, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(2f, 2f), 0, EroicaPalette.Gold, 0.8f);
                sparkle.noGravity = true;
            }

            Lighting.AddLight(hitPos, accentColor.ToVector3() * 0.6f * intensity);
        }

        /// <summary>
        /// Standard ambient lighting for any Eroica enemy. Apply per-frame from AI.
        /// </summary>
        public static void EroicaEnemyLight(Vector2 pos, Color color, float intensity)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Lighting.AddLight(pos, color.ToVector3() * intensity * pulse);
        }

        #endregion
    }
}
