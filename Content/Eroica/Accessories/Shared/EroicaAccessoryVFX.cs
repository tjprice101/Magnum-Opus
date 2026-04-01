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

namespace MagnumOpus.Content.Eroica.Accessories.Shared
{
    // ═══════════════ BADGE OF VALOR (Shared Tier 1/2 theme accessory) ═══════════════
    // Identity: Orbiting leaves, petals, embers. Universal Eroica badge.

    /// <summary>
    /// VFX for Badge of Valor / Hero's Symphony — universal Eroica theme accents.
    /// </summary>
    public static class BadgeOfValorVFX
    {
        private static readonly Color ValorGold = EroicaPalette.Gold;
        private static readonly Color ValorScarlet = EroicaPalette.Scarlet;
        private static readonly Color ValorHotCore = EroicaPalette.HotCore;

        /// <summary>
        /// Ambient orbiting valor embers and music notes. Every 8 frames: 4 orbiting embers
        /// alternating Scarlet/Gold with sine bob. Every 20 frames: orbiting music note.
        /// </summary>
        public static void AmbientBadgeOrbit(Vector2 playerCenter, float timer)
        {
            if ((int)timer % 8 == 0)
            {
                float baseAngle = timer * 0.035f;
            }
            if ((int)timer % 20 == 0)
                EroicaVFXLibrary.SpawnMusicNotes(playerCenter + new Vector2(0f, -12f), 1, 15f, 0.65f, 0.85f, 35);

            float pulse = 0.25f + MathF.Sin(timer * 0.03f) * 0.06f;
            Lighting.AddLight(playerCenter, ValorGold.ToVector3() * pulse);
        }

        /// <summary>
        /// Badge proc flash — on kill invulnerability grant. Golden bloom, 6-point radial
        /// ember burst, 3 gradient halo rings, music notes, dynamic lighting.
        /// </summary>
        public static void BadgeValorProcFlash(Vector2 center)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color burstColor = Color.Lerp(ValorScarlet, ValorGold, (float)i / 6f);
                Dust d = Dust.NewDustPerfect(center, DustID.Torch, vel, 0, burstColor, 1.4f);
                d.noGravity = true;
            }
            EroicaVFXLibrary.SpawnGradientHaloRings(center, 3, 0.3f);
            EroicaVFXLibrary.SpawnMusicNotes(center, 2, 15f, 0.75f, 0.95f, 30);
            Lighting.AddLight(center, ValorGold.ToVector3() * 1.2f);
        }
    }

    // ═══════════════ PYRE OF THE FALLEN HERO (Melee Accessory) ═══════════════
    // Identity: Fury flames that build with melee hits. 12 stacks -> 360 slash wave.

    /// <summary>
    /// VFX for Pyre of the Fallen Hero — melee fury stacking with escalating flame intensity.
    /// </summary>
    public static class PyreOfTheFallenHeroVFX
    {
        private static readonly Color PyreEmber = new Color(220, 80, 40);
        private static readonly Color FuryFlame = new Color(255, 120, 50);
        private static readonly Color RageGold = new Color(255, 200, 80);
        private static readonly Color InfernoCore = new Color(255, 240, 200);

        /// <summary>
        /// Ambient pyre aura — orbiting flame points (count = 2 + furyStacks/3), rising embers.
        /// Scales with fury: frame interval, color, density all intensify. >8 stacks: ring + notes.
        /// </summary>
        public static void AmbientPyreAura(Vector2 playerCenter, float timer, int furyStacks)
        {
            float furyIntensity = furyStacks / 12f;
            int frameInterval = Math.Max(2, 6 - furyStacks / 3);

            if ((int)timer % frameInterval == 0)
            {
                int flameCount = 2 + furyStacks / 3;
                float baseAngle = timer * (0.04f + furyIntensity * 0.02f);
                int emberCount = 1 + (int)(furyIntensity * 2);
                for (int i = 0; i < emberCount; i++)
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -0.8f - Main.rand.NextFloat(0.6f));
                    Color col = Color.Lerp(PyreEmber, RageGold, Main.rand.NextFloat() * furyIntensity);
                    Dust d = Dust.NewDustPerfect(playerCenter + Main.rand.NextVector2Circular(22f, 22f),
                        DustID.Torch, vel, 0, col, 1.0f + furyIntensity * 0.4f);
                    d.noGravity = true;
                }
            }
            if (furyStacks > 8)
            {
                if ((int)timer % 4 == 0)
                if ((int)timer % 15 == 0)
                    EroicaVFXLibrary.SpawnMusicNotes(playerCenter, 1, 10f, 0.7f, 0.9f, 25);
            }
            float lightIntensity = 0.2f + furyIntensity * 0.25f;
            Lighting.AddLight(playerCenter, Color.Lerp(PyreEmber, RageGold, furyIntensity).ToVector3() * lightIntensity);
        }

        /// <summary>
        /// Per-hit fury stack feedback — flame burst scaling with stacks (3 + stacks/2 dust),
        /// color shifts PyreEmber -> FuryFlame -> RageGold. Music note accent every 4th stack.
        /// </summary>
        public static void FuryStackVFX(Vector2 playerCenter, int furyStacks)
        {
            float progress = furyStacks / 12f;
            int dustCount = 3 + furyStacks / 2;
            Color stackColor = progress < 0.5f
                ? Color.Lerp(PyreEmber, FuryFlame, progress * 2f)
                : Color.Lerp(FuryFlame, RageGold, (progress - 0.5f) * 2f);

            for (int i = 0; i < dustCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f + progress * 2f, 3f + progress * 2f);
                Dust d = Dust.NewDustPerfect(playerCenter + Main.rand.NextVector2Circular(15f, 15f),
                    DustID.Torch, vel, 0, stackColor, 1.0f + progress * 0.5f);
                d.noGravity = true;
            }
            if (furyStacks % 4 == 0 && furyStacks > 0)
                EroicaVFXLibrary.SpawnMusicNotes(playerCenter, 1, 8f, 0.7f, 0.85f, 20);
        }

        /// <summary>
        /// Max fury release — MASSIVE 3-layer bloom, 5 shockwave rings, 30-particle 360 ember spray,
        /// 8-note radial burst, 6 god rays (PyreEmber/RageGold), screen shake (6f), screen ripple.
        /// </summary>
        public static void FuryReleasePulse(Vector2 center)
        {
            EroicaVFXLibrary.SpawnGradientHaloRings(center, 5, 0.35f);
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                Color sprayColor = Color.Lerp(PyreEmber, RageGold, (float)i / 30f);
                Dust d = Dust.NewDustPerfect(center, DustID.Torch, vel, 0, sprayColor, 1.5f);
                d.noGravity = true;
            }
            EroicaVFXLibrary.MusicNoteBurst(center, RageGold, 8, 4f);
            GodRaySystem.CreateBurst(center, PyreEmber, 6, 90f, 35, GodRaySystem.GodRayStyle.Explosion, RageGold);
            ScreenDistortionManager.TriggerRipple(center, PyreEmber, 0.7f, 20);
            Lighting.AddLight(center, InfernoCore.ToVector3() * 2f);
        }

        /// <summary>
        /// Active damage boost indicator — pulsing gold ring, rising ember motes, notes every 20f.
        /// </summary>
        public static void DamageBoostIndicator(Vector2 playerCenter, int timerRemaining)
        {
            float progress = timerRemaining / 120f;
            if (timerRemaining % 6 == 0)
            if (timerRemaining % 8 == 0)
            {
                Vector2 vel = new Vector2(0f, -0.6f - Main.rand.NextFloat(0.4f));
                Dust d = Dust.NewDustPerfect(playerCenter + Main.rand.NextVector2Circular(18f, 18f),
                    DustID.Torch, vel, 0, RageGold * 0.7f, 0.9f);
                d.noGravity = true;
            }
            if (timerRemaining % 20 == 0)
                EroicaVFXLibrary.SpawnMusicNotes(playerCenter + new Vector2(0f, -10f), 1, 10f, 0.6f, 0.8f, 25);
        }
    }

    // ═══════════════ SAKURA'S BURNING WILL (Summoner Accessory) ═══════════════
    // Identity: Sakura minion summoner, petal aura, defense proximity glow.

    /// <summary>
    /// VFX for Sakura's Burning Will — sakura petal aesthetics, spirit summons, proximity glow.
    /// </summary>
    public static class SakurasBurningWillVFX
    {
        private static readonly Color WillSakura = new Color(255, 140, 170);
        private static readonly Color BurningPetal = new Color(255, 100, 130);
        private static readonly Color SummonGlow = new Color(220, 180, 140);
        private static readonly Color SpiritFlame = new Color(200, 60, 80);

        /// <summary>
        /// Ambient sakura aura — every 10f: 3 floating petal motes (WillSakura/BurningPetal).
        /// Every 20f: sakura music note. Pulsing SpiritFlame lighting.
        /// </summary>
        public static void AmbientSakuraWillAura(Vector2 playerCenter, float timer)
        {
            if ((int)timer % 10 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.4f - Main.rand.NextFloat(0.4f));
                    Color petalColor = Color.Lerp(WillSakura, BurningPetal, Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(playerCenter + Main.rand.NextVector2Circular(25f, 25f),
                        DustID.PinkTorch, vel, 0, petalColor, 1.1f);
                    d.noGravity = true;
                }
            }
            if ((int)timer % 20 == 0)
                EroicaVFXLibrary.SpawnSakuraMusicNotes(playerCenter + new Vector2(0f, -10f), 1, 12f);

            float pulse = 0.2f + MathF.Sin(timer * 0.035f) * 0.05f;
            Lighting.AddLight(playerCenter, SpiritFlame.ToVector3() * pulse);
        }

        /// <summary>
        /// Spirit summon VFX — 25-point radial crimson flame burst, bloom flash,
        /// 3 gradient halos, 8-petal converging ring, 3 sakura music notes, screen trauma.
        /// </summary>
        public static void HeroicSpiritSummonVFX(Vector2 spawnPos)
        {
            for (int i = 0; i < 25; i++)
            {
                float angle = MathHelper.TwoPi * i / 25f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.CrimsonTorch, vel, 0, SpiritFlame, 1.6f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
            EroicaVFXLibrary.SpawnGradientHaloRings(spawnPos, 3, 0.25f);
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 startPos = spawnPos + angle.ToRotationVector2() * 40f;
                Vector2 vel = (spawnPos - startPos) * 0.08f;
                Dust d = Dust.NewDustPerfect(startPos, DustID.PinkTorch, vel, 0, WillSakura, 1.3f);
                d.noGravity = true;
            }
            EroicaVFXLibrary.SpawnSakuraMusicNotes(spawnPos, 3, 20f);
            Lighting.AddLight(spawnPos, WillSakura.ToVector3() * 1.0f);
        }

        /// <summary>
        /// Minion proximity defense glow — 3 orbiting WillSakura motes + subtle defense dust.
        /// Indicates the +8 defense proximity bonus is active.
        /// </summary>
        public static void MinionProximityGlow(Vector2 playerCenter, bool nearMinion)
        {
            if (!nearMinion) return;
            float orbitAngle = Main.GameUpdateCount * 0.05f;
            for (int i = 0; i < 3; i++)
            {
                float angle = orbitAngle + MathHelper.TwoPi * i / 3f;
                float radius = 22f + MathF.Sin(Main.GameUpdateCount * 0.03f) * 3f;
                Vector2 motePos = playerCenter + angle.ToRotationVector2() * radius;
                if (Main.GameUpdateCount % 6 == 0)
                {
                    Dust d = Dust.NewDustPerfect(motePos, DustID.PinkTorch, Vector2.Zero, 0, WillSakura * 0.6f, 0.8f);
                    d.noGravity = true;
                }
            }
            if (Main.GameUpdateCount % 12 == 0)
            {
                Dust d = Dust.NewDustPerfect(playerCenter + Main.rand.NextVector2Circular(15f, 15f),
                    DustID.Enchanted_Pink, new Vector2(0f, -0.3f), 0, SummonGlow * 0.5f, 0.7f);
                d.noGravity = true;
            }
        }
    }

    // ═══════════════ FUNERAL MARCH INSIGNIA (Mage Accessory) ═══════════════
    // Identity: Death prevention + invulnerability. Dark, dramatic funeral march.

    /// <summary>
    /// VFX for Funeral March Insignia — somber death-prevention accessory with dark embers,
    /// smoke wisps, and devastating Heroic Encore activation effects.
    /// </summary>
    public static class FuneralMarchInsigniaVFX
    {
        private static readonly Color InsigniaScarlet = new Color(180, 40, 40);
        private static readonly Color MarchBlack = new Color(30, 15, 20);
        private static readonly Color EncoreGold = new Color(255, 200, 100);
        private static readonly Color UndyingWhite = new Color(255, 245, 240);

        /// <summary>
        /// Ambient funeral aura — every 12f: 2 slow orbiting dark ember motes + smoke wisps.
        /// Every 30f: somber music note. Dim InsigniaScarlet lighting.
        /// </summary>
        public static void AmbientFuneralAura(Vector2 playerCenter, float timer)
        {
            if ((int)timer % 12 == 0)
            {
                float baseAngle = timer * 0.02f;
                for (int i = 0; i < 2; i++)
                {
                    float angle = baseAngle + MathHelper.Pi * i;
                    float radius = 28f + MathF.Sin(timer * 0.03f + i) * 4f;
                    Vector2 motePos = playerCenter + angle.ToRotationVector2() * radius;
                    Color moteColor = Color.Lerp(MarchBlack, InsigniaScarlet, (float)i / 2f);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.CrimsonTorch, Vector2.Zero, 80, moteColor, 0.9f);
                    d.noGravity = true;
                }
                var smoke = new HeavySmokeParticle(
                    playerCenter + Main.rand.NextVector2Circular(20f, 20f),
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.5f),
                    MarchBlack, 30, 0.4f, 0.25f, 0.01f, false, 0f, true);
            }
            if ((int)timer % 30 == 0)
                EroicaVFXLibrary.SpawnMusicNotes(playerCenter + new Vector2(0f, -15f), 1, 12f, 0.6f, 0.8f, 40);

            float pulse = 0.15f + MathF.Sin(timer * 0.025f) * 0.04f;
            Lighting.AddLight(playerCenter, InsigniaScarlet.ToVector3() * pulse);
        }

        /// <summary>
        /// DEATH PREVENTION — Heroic Encore activation. 3-ring flame burst (30 dust/ring),
        /// 40 black smoke particles, 20 golden sparks, UndyingWhite bloom,
        /// 8 god rays, 10-note cascade, screen shake (8f), screen ripple.
        /// </summary>
        public static void HeroicEncoreActivation(Vector2 playerCenter)
        {
            // 3-ring flame burst with escalating speed
            for (int ring = 0; ring < 3; ring++)
            {
                float speed = 6f + ring * 3f;
                for (int i = 0; i < 30; i++)
                {
                    float angle = MathHelper.TwoPi * i / 30f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.7f, speed);
                    Color flameColor = Color.Lerp(InsigniaScarlet, EncoreGold, (float)ring / 3f);
                    Dust d = Dust.NewDustPerfect(playerCenter, DustID.CrimsonTorch, vel, 0, flameColor, 2.2f - ring * 0.3f);
                    d.noGravity = true;
                    d.fadeIn = 1.4f;
                }
            }
            // Black smoke explosion
            for (int i = 0; i < 40; i++)
            {
                var smoke = new HeavySmokeParticle(
                    playerCenter + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2Circular(8f, 8f),
                    MarchBlack, 40, Main.rand.NextFloat(0.6f, 1.2f),
                    0.6f, Main.rand.NextFloat(-0.03f, 0.03f), false, 0f, true);
            }
            // Golden spark spray
            for (int i = 0; i < 20; i++)
            {
                Dust d = Dust.NewDustPerfect(playerCenter, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(8f, 8f), 0, EncoreGold, 1.4f);
                d.noGravity = true;
            }
            GodRaySystem.CreateBurst(playerCenter, InsigniaScarlet, 8, 120f, 40,
                GodRaySystem.GodRayStyle.Explosion, EncoreGold);
            EroicaVFXLibrary.MusicNoteBurst(playerCenter, InsigniaScarlet, 10, 5f);
            ScreenDistortionManager.TriggerRipple(playerCenter, InsigniaScarlet, 1.0f, 30);
            Lighting.AddLight(playerCenter, UndyingWhite.ToVector3() * 2.5f);
        }

        /// <summary>
        /// Active Heroic Encore VFX during invulnerability. Every 2f: scarlet flame aura,
        /// every 4f: black smoke trail, every 6f: golden glow ring, every 10f: music notes.
        /// </summary>
        public static void HeroicEncoreActiveVFX(Vector2 playerCenter, int timer)
        {
            float progress = timer / 180f;
            if (timer % 2 == 0)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -2f - Main.rand.NextFloat(1.5f));
                Color flameColor = Color.Lerp(InsigniaScarlet, EncoreGold, Main.rand.NextFloat() * 0.5f);
                Dust d = Dust.NewDustPerfect(playerCenter + Main.rand.NextVector2Circular(25f, 35f),
                    DustID.CrimsonTorch, vel, 0, flameColor, 1.8f);
                d.noGravity = true;
            }
            if (timer % 4 == 0)
            {
                var smoke = new HeavySmokeParticle(
                    playerCenter + Main.rand.NextVector2Circular(18f, 28f),
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1.5f),
                    MarchBlack, 25, 0.5f, 0.4f, 0.02f, false, 0f, false);
            }
            if (timer % 6 == 0)
            if (timer % 10 == 0)
                EroicaVFXLibrary.SpawnMusicNotes(playerCenter, 1, 15f, 0.75f, 1.0f, 25);

            float lightIntensity = 0.8f + (1f - progress) * 0.6f;
            Color lightColor = Color.Lerp(InsigniaScarlet, EncoreGold, MathF.Sin(timer * 0.1f) * 0.5f + 0.5f);
            Lighting.AddLight(playerCenter, lightColor.ToVector3() * lightIntensity);
        }

        /// <summary>
        /// Low mana warning (below 20%) — pulsing InsigniaScarlet halo, rising prayer dust.
        /// Urgency scales inversely with remaining mana percentage.
        /// </summary>
        public static void LowManaWarning(Vector2 playerCenter, float manaPercent)
        {
            if (manaPercent >= 0.2f) return;
            float urgency = 1f - (manaPercent / 0.2f);

            if (Main.GameUpdateCount % 10 == 0)
            if (Main.GameUpdateCount % 8 == 0)
            {
                Vector2 vel = new Vector2(0f, -0.4f - Main.rand.NextFloat(0.3f));
                Dust d = Dust.NewDustPerfect(playerCenter + Main.rand.NextVector2Circular(15f, 15f),
                    DustID.CrimsonTorch, vel, 100, InsigniaScarlet * (0.4f + urgency * 0.3f), 0.7f + urgency * 0.3f);
                d.noGravity = true;
            }
        }
    }

    // ═══════════════ SYMPHONY OF SCARLET FLAMES (Ranger Accessory) ═══════════════
    // Identity: Consecutive hit marking -> precision explosion on 4th hit same target.

    /// <summary>
    /// VFX for Symphony of Scarlet Flames — ranger precision marking and detonation system.
    /// </summary>
    public static class SymphonyOfScarletFlamesVFX
    {
        private static readonly Color SymphonyScarlet = new Color(210, 50, 50);
        private static readonly Color PrecisionGold = new Color(255, 210, 80);
        private static readonly Color MarkCrimson = new Color(180, 35, 45);
        private static readonly Color DetonationWhite = new Color(255, 245, 235);

        /// <summary>
        /// Ambient symphony aura — every 8f: 2 orbiting SymphonyScarlet flame motes.
        /// Every 25f: scarlet music note. Subtle ambient glow.
        /// </summary>
        public static void AmbientSymphonyAura(Vector2 playerCenter, float timer)
        {
            if ((int)timer % 8 == 0)
            {
                float baseAngle = timer * 0.045f;
                for (int i = 0; i < 2; i++)
                {
                    float angle = baseAngle + MathHelper.Pi * i;
                    float radius = 24f + MathF.Sin(timer * 0.05f + i * 2f) * 4f;
                    Vector2 motePos = playerCenter + angle.ToRotationVector2() * radius;
                    Dust d = Dust.NewDustPerfect(motePos, DustID.CrimsonTorch,
                        angle.ToRotationVector2() * 0.2f, 0, SymphonyScarlet * 0.7f, 0.9f);
                    d.noGravity = true;
                }
            }
            if ((int)timer % 25 == 0)
                EroicaVFXLibrary.SpawnMusicNotes(playerCenter + new Vector2(0f, -10f), 1, 10f, 0.65f, 0.85f, 30);

            float pulse = 0.18f + MathF.Sin(timer * 0.03f) * 0.04f;
            Lighting.AddLight(playerCenter, SymphonyScarlet.ToVector3() * pulse);
        }

        /// <summary>
        /// Target mark VFX — escalating dust burst (5 + hits*3), color shift
        /// PrecisionGold -> MarkCrimson -> SymphonyScarlet. Rotating crosshair at 3+ hits.
        /// </summary>
        public static void TargetMarkVFX(Vector2 targetCenter, int consecutiveHits)
        {
            float progress = Math.Min(consecutiveHits / 3f, 1f);
            int dustCount = 5 + consecutiveHits * 3;
            Color markColor = progress < 0.5f
                ? Color.Lerp(PrecisionGold, MarkCrimson, progress * 2f)
                : Color.Lerp(MarkCrimson, SymphonyScarlet, (progress - 0.5f) * 2f);

            for (int i = 0; i < dustCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f + consecutiveHits, 2f + consecutiveHits);
                int dustType = Main.rand.NextBool() ? DustID.CrimsonTorch : DustID.GoldFlame;
                Dust d = Dust.NewDustPerfect(targetCenter + Main.rand.NextVector2Circular(12f, 12f),
                    dustType, vel, 0, markColor, 1.0f + consecutiveHits * 0.15f);
                d.noGravity = true;
            }

            // Rotating crosshair at 3+ hits — primed indicator
            if (consecutiveHits >= 3)
            {
                float rotation = Main.GameUpdateCount * 0.08f;
                for (int i = 0; i < 4; i++)
                {
                    float crossAngle = rotation + MathHelper.PiOver2 * i;
                    Vector2 crossPos = targetCenter + crossAngle.ToRotationVector2() * 18f;
                    Dust d = Dust.NewDustPerfect(crossPos, DustID.CrimsonTorch, Vector2.Zero, 0, SymphonyScarlet, 1.0f);
                    d.noGravity = true;
                }
            }
            if (consecutiveHits >= 2)
                EroicaVFXLibrary.SpawnMusicNotes(targetCenter, 1, 8f, 0.7f, 0.85f, 20);
        }

        /// <summary>
        /// 4th-hit precision detonation — DetonationWhite bloom, 6-point crimson star burst,
        /// 4 halo ring cascade, 25 radial dust, 6-note burst, 4 god rays, shake (4f), ripple.
        /// </summary>
        public static void PrecisionDetonationVFX(Vector2 targetCenter)
        {
            // 6-point crimson star burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 dir = angle.ToRotationVector2();
                for (int j = 0; j < 3; j++)
                {
                    Vector2 vel = dir * (3f + j * 3f);
                    Color rayColor = Color.Lerp(SymphonyScarlet, DetonationWhite, (float)j / 3f);
                    Dust d = Dust.NewDustPerfect(targetCenter, DustID.CrimsonTorch, vel, 0, rayColor, 1.4f - j * 0.1f);
                    d.noGravity = true;
                }
            }
            // 4 halo ring cascade
            for (int i = 0; i < 4; i++)
            {
                Color ringColor = Color.Lerp(SymphonyScarlet, PrecisionGold, (float)i / 4f);
                var ring = new BloomRingParticle(targetCenter, Vector2.Zero, ringColor,
                    0.25f + i * 0.12f, 20 + i * 3, 0.1f);
            }
            EroicaVFXLibrary.SpawnRadialDustBurst(targetCenter, 25, 7f, DustID.CrimsonTorch);
            EroicaVFXLibrary.MusicNoteBurst(targetCenter, SymphonyScarlet, 6, 3.5f);
            GodRaySystem.CreateBurst(targetCenter, SymphonyScarlet, 4, 70f, 25,
                GodRaySystem.GodRayStyle.Explosion, PrecisionGold);
            ScreenDistortionManager.TriggerRipple(targetCenter, SymphonyScarlet, 0.5f, 15);
            Lighting.AddLight(targetCenter, DetonationWhite.ToVector3() * 1.8f);
        }

        /// <summary>
        /// First hit on new target — small 5-particle gold mark indicator + accent note.
        /// </summary>
        public static void NewTargetMarkVFX(Vector2 targetCenter)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustPerfect(targetCenter, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(2f, 2f), 0, PrecisionGold, 1.0f);
                d.noGravity = true;
            }
            EroicaVFXLibrary.SpawnMusicNotes(targetCenter, 1, 6f, 0.6f, 0.75f, 18);
        }
    }
}
