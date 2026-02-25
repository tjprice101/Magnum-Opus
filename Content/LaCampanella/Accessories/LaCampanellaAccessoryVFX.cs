using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.LaCampanella.Accessories
{
    // =============================================================================
    //  CHAMBER OF BELLFIRE (Tier 1)
    //  Identity: Infernal aura, bellfire shell, converging fire, bell explosion proc.
    // =============================================================================

    /// <summary>
    /// VFX for Chamber of Bellfire — Tier 1 infernal aura accessory.
    /// Fire resistance aura, bellfire damage, bell explosion every 10 hits.
    /// </summary>
    public static class ChamberOfBellfireVFX
    {
        private static readonly Color BellfireCore = LaCampanellaPalette.InfernalOrange;
        private static readonly Color BellfireShell = LaCampanellaPalette.DeepEmber;
        private static readonly Color BellfireFlash = LaCampanellaPalette.FlameYellow;

        /// <summary>
        /// Ambient bellfire aura — orbiting converging fire motes, rising embers,
        /// smoke wisps, periodic bell shimmer. Lower density than higher tiers.
        /// </summary>
        public static void AmbientBellfireAura(Vector2 playerCenter, float timer)
        {
            // Converging fire motes (6-frame interval)
            if ((int)timer % 6 == 0)
            {
                float baseAngle = timer * 0.03f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 30f + MathF.Sin(timer * 0.04f + i * 1.5f) * 6f;
                    Vector2 motePos = playerCenter + angle.ToRotationVector2() * radius;
                    Vector2 toCenter = (playerCenter - motePos).SafeNormalize(Vector2.Zero) * 0.8f;
                    Color col = Color.Lerp(BellfireShell, BellfireCore, (float)i / 3f);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.Torch, toCenter, 0, col, 0.9f);
                    d.noGravity = true;
                    d.fadeIn = 0.5f;
                }
            }

            // Rising embers
            if ((int)timer % 8 == 0)
            {
                Vector2 pos = playerCenter + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.8f - Main.rand.NextFloat(0.5f));
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.8f);
                d.noGravity = true;
            }

            // Smoke wisps
            LaCampanellaVFXLibrary.SpawnAmbientSmoke(playerCenter, 25f);

            // Periodic bell shimmer
            if ((int)timer % 15 == 0)
            {
                Color shimmer = LaCampanellaPalette.GetBellShimmer(timer);
                Dust d = Dust.NewDustPerfect(playerCenter + Main.rand.NextVector2Circular(18f, 18f),
                    DustID.Enchanted_Gold, Vector2.Zero, 0, shimmer, 0.5f);
                d.noGravity = true;
            }

            // Sparse music notes
            if ((int)timer % 30 == 0)
                LaCampanellaVFXLibrary.SpawnMusicNotes(playerCenter, 1, 25f, 0.7f, 0.85f, 30);

            float pulse = 0.3f + MathF.Sin(timer * 0.04f) * 0.08f;
            Lighting.AddLight(playerCenter, BellfireCore.ToVector3() * pulse);
        }

        /// <summary>
        /// Aura burn VFX — fire particles on enemy damaged by bellfire aura tick.
        /// </summary>
        public static void AuraBurnVFX(Vector2 enemyCenter)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -0.5f);
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(enemyCenter + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.Torch, vel, 0, col, 1.0f);
                d.noGravity = true;
            }
            Lighting.AddLight(enemyCenter, BellfireCore.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Signature hit VFX — per-hit accent with ember scatter and smoke sparkle.
        /// </summary>
        public static void SignatureHitVFX(Vector2 hitPos)
        {
            LaCampanellaVFXLibrary.SpawnEmberScatter(hitPos, 3, 2f);

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Torch, vel, 0, BellfireCore, 1.1f);
                d.noGravity = true;
            }

            if (Main.rand.NextBool(3))
                LaCampanellaVFXLibrary.SpawnMusicNotes(hitPos, 1, 8f, 0.7f, 0.85f, 20);
        }

        /// <summary>
        /// Bell explosion proc VFX — every 10 hits, massive bell-fire eruption.
        /// Central bloom flash, radial fire burst, bell chime rings, ember debris,
        /// music notes, heavy smoke cloud, screen shake.
        /// </summary>
        public static void BellExplosionVFX(Vector2 pos)
        {
            // Core bloom flash
            LaCampanellaVFXLibrary.DrawBloom(pos, 0.8f);

            // Radial fire burst (16-point)
            LaCampanellaVFXLibrary.SpawnRadialDustBurst(pos, 16, 7f);

            // Bell chime rings cascade
            LaCampanellaVFXLibrary.SpawnBellChimeRings(pos, 3, 0.35f);
            CustomParticles.LaCampanellaBellChime(pos, 12);

            // Ember debris
            LaCampanellaVFXLibrary.SpawnEmberScatter(pos, 10, 5f);

            // Music notes cascade
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 5, 35f, 0.8f, 1.1f, 35);

            // Heavy smoke cloud
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 6, 0.9f, 3.5f, 55);

            // Screen shake
            MagnumScreenEffects.AddScreenShake(4f);
            Lighting.AddLight(pos, LaCampanellaPalette.WhiteHot.ToVector3() * 1.2f);
        }

        /// <summary>
        /// VFX on each enemy hit by bell explosion AOE.
        /// </summary>
        public static void BellExplosionHitVFX(Vector2 enemyCenter, Vector2 fromCenter)
        {
            Vector2 dir = (enemyCenter - fromCenter).SafeNormalize(Vector2.UnitX);
            LaCampanellaVFXLibrary.SpawnEmberScatter(enemyCenter, 4, 3f);
            for (int i = 0; i < 3; i++)
            {
                Dust d = Dust.NewDustPerfect(enemyCenter, DustID.Torch,
                    dir * Main.rand.NextFloat(2f, 4f) + Main.rand.NextVector2Circular(1f, 1f),
                    0, BellfireFlash, 1.2f);
                d.noGravity = true;
            }
        }
    }

    // =============================================================================
    //  CAMPANELLA'S PYRE MEDALLION (Tier 2)
    //  Identity: Crit-focused, flame trail on dash, Resonant Toll synergy.
    // =============================================================================

    /// <summary>
    /// VFX for Campanella's Pyre Medallion — crit and attack speed accessory
    /// with flame trail on dash and Resonant Toll damage bonus.
    /// </summary>
    public static class CampanellasPyreMedallionVFX
    {
        private static readonly Color PyreOrange = LaCampanellaPalette.InfernalOrange;
        private static readonly Color PyreCrit = LaCampanellaPalette.FlameYellow;
        private static readonly Color PyreFlash = LaCampanellaPalette.BellGold;

        /// <summary>
        /// Ambient pyre aura — orbiting flame points with crit-spark accents.
        /// Tighter orbit, sharper motes to convey precision/speed.
        /// </summary>
        public static void AmbientPyreAura(Vector2 playerCenter, float timer)
        {
            // Fast-orbiting flame points (2 points, faster spin)
            if ((int)timer % 5 == 0)
            {
                float baseAngle = timer * 0.05f;
                for (int i = 0; i < 2; i++)
                {
                    float angle = baseAngle + MathHelper.Pi * i;
                    float radius = 24f + MathF.Sin(timer * 0.06f + i * 2f) * 4f;
                    Vector2 flamePos = playerCenter + angle.ToRotationVector2() * radius;
                    Color col = Color.Lerp(PyreOrange, PyreCrit, (float)i / 2f);
                    Dust d = Dust.NewDustPerfect(flamePos, DustID.Torch,
                        angle.ToRotationVector2() * 0.3f, 0, col, 0.85f);
                    d.noGravity = true;
                }
            }

            // Crit sparkle accent
            if ((int)timer % 12 == 0)
            {
                Dust d = Dust.NewDustPerfect(
                    playerCenter + Main.rand.NextVector2Circular(15f, 15f),
                    DustID.GoldFlame, new Vector2(0, -0.5f), 0, PyreFlash, 0.7f);
                d.noGravity = true;
            }

            // Music note accent
            if ((int)timer % 25 == 0)
                LaCampanellaVFXLibrary.SpawnMusicNotes(playerCenter + new Vector2(0f, -10f), 1, 12f, 0.65f, 0.85f, 30);

            float pulse = 0.25f + MathF.Sin(timer * 0.04f) * 0.06f;
            Lighting.AddLight(playerCenter, PyreOrange.ToVector3() * pulse);
        }

        /// <summary>
        /// Dash flame trail VFX — dense fire trail when player is moving fast (dashing).
        /// </summary>
        public static void DashFlameTrailVFX(Vector2 trailPos, Vector2 velocity)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2f)
                    + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(trailPos + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.Torch, vel, 0, col, 1.3f);
                d.noGravity = true;
                d.fadeIn = 0.3f;
            }

            // Smoke trail behind
            if (Main.rand.NextBool(2))
                LaCampanellaVFXLibrary.SpawnHeavySmoke(trailPos, 1, 0.3f, 1f, 25);
        }

        /// <summary>
        /// Resonant Toll bonus hit VFX — extra fire accent when hitting enemies with stacks.
        /// </summary>
        public static void ResonantTollBonusHitVFX(Vector2 hitPos, int stacks)
        {
            float intensity = Math.Min(stacks / 5f, 1f);
            int dustCount = 3 + (int)(intensity * 4);

            for (int i = 0; i < dustCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f + intensity * 2f, 3f + intensity * 2f);
                Color col = Color.Lerp(PyreOrange, PyreCrit, intensity);
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Torch, vel, 0, col, 1.0f + intensity * 0.4f);
                d.noGravity = true;
            }

            if (stacks >= 3)
                LaCampanellaVFXLibrary.SpawnMusicNotes(hitPos, 1, 8f, 0.7f, 0.85f, 20);
        }
    }

    // =============================================================================
    //  SYMPHONY OF THE BLAZING SANCTUARY (Tier 3)
    //  Identity: Protective bell barrier, healing pillars, defensive warmth.
    // =============================================================================

    /// <summary>
    /// VFX for Symphony of the Blazing Sanctuary — defensive Tier 3 accessory.
    /// Bell barrier on low HP (70% DR), healing fire pillar on kill, regen aura.
    /// </summary>
    public static class SymphonyOfTheBlazingSanctuaryVFX
    {
        private static readonly Color SanctuaryGold = LaCampanellaPalette.BellGold;
        private static readonly Color BarrierOrange = LaCampanellaPalette.InfernalOrange;
        private static readonly Color HealingFlame = LaCampanellaPalette.FlameYellow;
        private static readonly Color ShieldWhite = LaCampanellaPalette.WhiteHot;

        /// <summary>
        /// Ambient sanctuary aura — protective orbiting ring, warm healing glow,
        /// gentle flame motes. Calm and sheltering compared to offensive accessories.
        /// </summary>
        public static void AmbientSanctuaryAura(Vector2 playerCenter, float timer)
        {
            // Protective orbiting ring (4-point golden shield pattern)
            if ((int)timer % 8 == 0)
            {
                float baseAngle = timer * 0.025f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 4f;
                    float radius = 35f + MathF.Sin(timer * 0.03f + i * 1.5f) * 4f;
                    Vector2 shieldPos = playerCenter + angle.ToRotationVector2() * radius;
                    Dust d = Dust.NewDustPerfect(shieldPos, DustID.GoldFlame,
                        angle.ToRotationVector2() * 0.2f, 0, SanctuaryGold * 0.7f, 0.85f);
                    d.noGravity = true;
                }
            }

            // Warm healing embers (slow rising)
            if ((int)timer % 10 == 0)
            {
                Vector2 pos = playerCenter + Main.rand.NextVector2Circular(22f, 22f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -0.5f - Main.rand.NextFloat(0.3f));
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, HealingFlame * 0.8f, 0.9f);
                d.noGravity = true;
            }

            // Smoke wisps
            LaCampanellaVFXLibrary.SpawnAmbientSmoke(playerCenter, 30f);

            // Music notes (gentle, slow)
            if ((int)timer % 25 == 0)
                LaCampanellaVFXLibrary.SpawnMusicNotes(playerCenter + new Vector2(0f, -12f), 1, 18f, 0.65f, 0.85f, 35);

            float pulse = 0.28f + MathF.Sin(timer * 0.03f) * 0.06f;
            Lighting.AddLight(playerCenter, SanctuaryGold.ToVector3() * pulse);
        }

        /// <summary>
        /// Bell Barrier activation VFX — MASSIVE defensive proc.
        /// Multi-ring shockwave, shield bloom flash, golden fire ring,
        /// heavy smoke explosion, music note cascade, screen shake.
        /// </summary>
        public static void BellBarrierActivationVFX(Vector2 center)
        {
            // Massive bloom flash
            LaCampanellaVFXLibrary.DrawBloom(center, 1.2f);

            // Bell shockwave impact
            LaCampanellaVFXLibrary.BellShockwaveImpact(center, 1.5f);

            // Extra cascading bell chime rings
            for (int i = 0; i < 4; i++)
            {
                CustomParticles.LaCampanellaBellChime(center, 12 + i * 4);
            }

            // 36-point golden fire ring (shield perimeter)
            for (int i = 0; i < 36; i++)
            {
                float angle = MathHelper.TwoPi * i / 36f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                Color col = Color.Lerp(BarrierOrange, SanctuaryGold, (float)i / 36f);
                Dust d = Dust.NewDustPerfect(center, DustID.Torch, vel, 0, col, 1.8f);
                d.noGravity = true;
            }

            // Heavy smoke explosion
            LaCampanellaVFXLibrary.SpawnHeavySmoke(center, 12, 1.2f, 5f, 70);

            // Ember debris
            LaCampanellaVFXLibrary.SpawnEmberScatter(center, 15, 6f);

            // Music note cascade
            LaCampanellaVFXLibrary.SpawnMusicNotes(center, 10, 50f, 0.8f, 1.2f, 40);

            MagnumScreenEffects.AddScreenShake(8f);
            Lighting.AddLight(center, ShieldWhite.ToVector3() * 2f);
        }

        /// <summary>
        /// Healing pillar VFX — fire column ascending from kill position.
        /// Vertical fire dust, bell chime at base, warm golden glow.
        /// </summary>
        public static void HealingPillarVFX(Vector2 basePos)
        {
            // Vertical fire column (20 tiers)
            for (int i = 0; i < 20; i++)
            {
                float height = i * 8f;
                Vector2 pos = basePos + new Vector2(Main.rand.NextFloat(-8f, 8f), -height);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -2f - Main.rand.NextFloat(2f));
                Color col = LaCampanellaPalette.GetFireGradient((float)i / 20f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            // Bell chime at base
            CustomParticles.LaCampanellaBellChime(basePos, 8);
            LaCampanellaVFXLibrary.DrawBloom(basePos, 0.5f);

            // Music notes ascending
            LaCampanellaVFXLibrary.SpawnMusicNotes(basePos, 3, 15f, 0.7f, 1.0f, 35);

            // Smoke at base
            LaCampanellaVFXLibrary.SpawnHeavySmoke(basePos, 3, 0.6f, 2f, 40);

            MagnumScreenEffects.AddScreenShake(2f);
            Lighting.AddLight(basePos, HealingFlame.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Signature hit VFX — golden shield accent on attack hit.
        /// </summary>
        public static void SignatureHitVFX(Vector2 hitPos)
        {
            LaCampanellaVFXLibrary.SpawnEmberScatter(hitPos, 3, 2f);

            Dust shimmer = Dust.NewDustPerfect(hitPos, DustID.GoldFlame,
                Main.rand.NextVector2Circular(2f, 2f), 0, SanctuaryGold, 0.9f);
            shimmer.noGravity = true;

            if (Main.rand.NextBool(3))
                LaCampanellaVFXLibrary.SpawnMusicNotes(hitPos, 1, 8f, 0.7f, 0.85f, 20);
        }

        /// <summary>
        /// VFX on each enemy knocked back by bell barrier.
        /// </summary>
        public static void BarrierKnockbackVFX(Vector2 enemyCenter, Vector2 fromCenter)
        {
            Vector2 dir = (enemyCenter - fromCenter).SafeNormalize(Vector2.UnitX);
            LaCampanellaVFXLibrary.SpawnEmberScatter(enemyCenter, 5, 4f);
            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustPerfect(enemyCenter, DustID.Torch,
                    dir * Main.rand.NextFloat(3f, 6f) + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    0, SanctuaryGold, 1.3f);
                d.noGravity = true;
            }
        }
    }

    // =============================================================================
    //  INFERNAL BELL OF THE MAESTRO (Tier 4 Ultimate)
    //  Identity: Walking inferno, Grand Tolling every 5s, fire mastery, ultimate power.
    // =============================================================================

    /// <summary>
    /// VFX for Infernal Bell of the Maestro — the ultimate La Campanella accessory.
    /// Fire mastery, Grand Tolling every 5s (300px AOE), infernal aura, crit fire burst.
    /// The most visually impressive accessory in the La Campanella theme.
    /// </summary>
    public static class InfernalBellOfTheMaestroVFX
    {
        private static readonly Color MaestroOrange = LaCampanellaPalette.InfernalOrange;
        private static readonly Color MaestroGold = LaCampanellaPalette.BellGold;
        private static readonly Color MaestroFlame = LaCampanellaPalette.FlameYellow;
        private static readonly Color MaestroCore = LaCampanellaPalette.WhiteHot;

        /// <summary>
        /// Ambient maestro aura — the most intense accessory aura.
        /// 3-layer effect: inner fire swirl, mid bell resonance ring, outer smoke atmosphere.
        /// </summary>
        public static void AmbientMaestroAura(Vector2 playerCenter, float timer)
        {
            // Layer 1: Inner fire swirl (2 orbiting intense flames)
            float swirlAngle = timer * 0.05f;
            for (int i = 0; i < 2; i++)
            {
                float angle = swirlAngle + i * MathHelper.Pi;
                float radius = 28f + MathF.Sin(timer * 0.08f) * 6f;
                Vector2 flamePos = playerCenter + angle.ToRotationVector2() * radius;

                if ((int)timer % 3 == 0)
                {
                    Dust d = Dust.NewDustPerfect(flamePos, DustID.Torch,
                        angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 1.5f,
                        0, MaestroOrange, 1.4f);
                    d.noGravity = true;
                }
            }

            // Layer 2: Mid bell resonance ring (6-point golden shimmer)
            if ((int)timer % 6 == 0)
            {
                float ringAngle = timer * 0.02f;
                for (int i = 0; i < 6; i++)
                {
                    float angle = ringAngle + MathHelper.TwoPi * i / 6f;
                    Vector2 ringPos = playerCenter + angle.ToRotationVector2() * 40f;

                    if (Main.rand.NextBool(2))
                    {
                        Dust d = Dust.NewDustPerfect(ringPos, DustID.GoldFlame,
                            Vector2.Zero, 0, MaestroGold * 0.6f, 0.7f);
                        d.noGravity = true;
                    }
                }
            }

            // Layer 3: Outer smoke atmosphere
            LaCampanellaVFXLibrary.SpawnAmbientSmoke(playerCenter, 35f);

            // Rising embers
            if ((int)timer % 4 == 0)
            {
                Vector2 pos = playerCenter + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -1f - Main.rand.NextFloat(0.6f));
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            // Random sparks
            if ((int)timer % 3 == 0 && Main.rand.NextBool(2))
            {
                LaCampanellaVFXLibrary.SpawnEmberScatter(playerCenter, 1, 2f);
            }

            // Music notes (more frequent for ultimate)
            if ((int)timer % 18 == 0)
                LaCampanellaVFXLibrary.SpawnMusicNotes(playerCenter, 1, 20f, 0.7f, 0.9f, 30);

            // Bell shimmer
            if ((int)timer % 10 == 0)
            {
                Color shimmer = LaCampanellaPalette.GetBellShimmer(timer);
                Dust d = Dust.NewDustPerfect(playerCenter + Main.rand.NextVector2Circular(20f, 20f),
                    DustID.Enchanted_Gold, Vector2.Zero, 0, shimmer, 0.6f);
                d.noGravity = true;
            }

            float pulse = 0.4f + MathF.Sin(timer * 0.04f) * 0.1f;
            Lighting.AddLight(playerCenter, MaestroOrange.ToVector3() * pulse);
        }

        /// <summary>
        /// Grand Tolling VFX — MASSIVE every-5-second AOE bell explosion.
        /// The signature VFX of the ultimate accessory. Multi-ring shockwave,
        /// infernal eruption, bell chime cascade, 32-point radial fire burst,
        /// heavy smoke explosion, music note explosion, screen shake.
        /// </summary>
        public static void GrandTollingVFX(Vector2 center)
        {
            // Infernal eruption (the heaviest VFX in the library)
            LaCampanellaVFXLibrary.InfernalEruption(center, 1.5f);

            // Extra cascading bell chime rings
            for (int i = 0; i < 5; i++)
            {
                CustomParticles.LaCampanellaBellChime(center, 15 + i * 5);
            }

            // 32-point radial fire burst (massive)
            for (int i = 0; i < 32; i++)
            {
                float angle = MathHelper.TwoPi * i / 32f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(7f, 12f);
                Color col = LaCampanellaPalette.GetFireGradient((float)i / 32f);
                Dust d = Dust.NewDustPerfect(center, DustID.Torch, vel, 0, col, 2.0f);
                d.noGravity = true;
            }

            // Extra heavy smoke explosion
            LaCampanellaVFXLibrary.SpawnHeavySmoke(center, 14, 1.4f, 6f, 80);

            // Music note explosion
            LaCampanellaVFXLibrary.SpawnMusicNotes(center, 12, 60f, 0.8f, 1.3f, 45);

            MagnumScreenEffects.AddScreenShake(10f);
            Lighting.AddLight(center, MaestroCore.ToVector3() * 2.5f);
        }

        /// <summary>
        /// VFX on each enemy hit by Grand Tolling AOE wave.
        /// </summary>
        public static void GrandTollingHitVFX(Vector2 enemyCenter, Vector2 fromCenter)
        {
            Vector2 dir = (enemyCenter - fromCenter).SafeNormalize(Vector2.UnitX);
            LaCampanellaVFXLibrary.SpawnEmberScatter(enemyCenter, 4, 3f);

            for (int i = 0; i < 3; i++)
            {
                Dust d = Dust.NewDustPerfect(enemyCenter, DustID.Torch,
                    dir * Main.rand.NextFloat(2f, 5f) + Main.rand.NextVector2Circular(1f, 1f),
                    0, MaestroFlame, 1.3f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Aura burn VFX — fire on enemy damaged by passive infernal aura.
        /// Enhanced version with bell sound accent.
        /// </summary>
        public static void MaestroAuraBurnVFX(Vector2 enemyCenter)
        {
            LaCampanellaVFXLibrary.SpawnEmberScatter(enemyCenter, 3, 2.5f);

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2.5f, 2.5f) + new Vector2(0, -0.8f);
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(enemyCenter + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.Torch, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            Lighting.AddLight(enemyCenter, MaestroOrange.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Ultimate signature hit VFX — the Maestro's on-hit accent.
        /// Fire burst with golden sparkle accent.
        /// </summary>
        public static void SignatureHitVFX(Vector2 hitPos, Vector2 hitDirection)
        {
            LaCampanellaVFXLibrary.SpawnEmberScatter(hitPos, 4, 3f);

            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = hitDirection * Main.rand.NextFloat(2f, 4f)
                    + Main.rand.NextVector2Circular(2f, 2f);
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Torch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            // Golden sparkle accent
            Dust shimmer = Dust.NewDustPerfect(hitPos, DustID.GoldFlame,
                Main.rand.NextVector2Circular(2f, 2f), 0, MaestroGold, 0.8f);
            shimmer.noGravity = true;

            if (Main.rand.NextBool(2))
                LaCampanellaVFXLibrary.SpawnMusicNotes(hitPos, 1, 8f, 0.7f, 0.85f, 20);
        }

        /// <summary>
        /// Critical hit bonus VFX — fire burst on crit with bell ring flash.
        /// Bloom, radial fire, bell chime, ember spray, screen shake.
        /// </summary>
        public static void CritBonusVFX(Vector2 hitPos)
        {
            LaCampanellaVFXLibrary.DrawBloom(hitPos, 0.6f);

            // Radial fire burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color col = LaCampanellaPalette.GetFireGradient((float)i / 12f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Torch, vel, 0, col, 1.4f);
                d.noGravity = true;
            }

            // Bell chime ring
            CustomParticles.LaCampanellaBellChime(hitPos, 8);
            LaCampanellaVFXLibrary.SpawnEmberScatter(hitPos, 6, 3f);
            LaCampanellaVFXLibrary.SpawnMusicNotes(hitPos, 3, 15f, 0.8f, 1.0f, 25);

            MagnumScreenEffects.AddScreenShake(3f);
            Lighting.AddLight(hitPos, MaestroCore.ToVector3() * 1.2f);
        }
    }

    // =============================================================================
    //  CHIME OF FLAMES (Theme Tier 1)
    //  Identity: Smoky-black mage bell, fire trails, 8% stun proc.
    // =============================================================================

    /// <summary>
    /// VFX for Chime of Flames — Theme Tier 1 magic accessory.
    /// +15% magic damage, fire trails on magic attacks, 8% bell ring stun proc.
    /// </summary>
    public static class ChimeOfFlamesVFX
    {
        private static readonly Color ChimeOrange = LaCampanellaPalette.InfernalOrange;
        private static readonly Color ChimeYellow = LaCampanellaPalette.FlameYellow;
        private static readonly Color ChimeGold = LaCampanellaPalette.BellGold;
        private static readonly Color ChimeSmoke = LaCampanellaPalette.SootBlack;

        /// <summary>
        /// Ambient chime aura — smoky black particles with orange flame licks.
        /// Moodier, smokier than standard fire aura. Bell shimmer accents.
        /// </summary>
        public static void AmbientChimeAura(Vector2 playerCenter, float timer)
        {
            // Smoky black particles (slow rising)
            if ((int)timer % 8 == 0)
            {
                LaCampanellaVFXLibrary.SpawnHeavySmoke(playerCenter, 1, 0.3f, 0.8f, 30);
            }

            // Orange flame licks (sparse, tall)
            if ((int)timer % 7 == 0)
            {
                Vector2 pos = playerCenter + Main.rand.NextVector2Circular(22f, 22f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -1f - Main.rand.NextFloat(1f));
                Color col = Color.Lerp(ChimeOrange, ChimeYellow, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.9f);
                d.noGravity = true;
            }

            // Bell shimmer accent
            if ((int)timer % 20 == 0)
            {
                Color shimmer = LaCampanellaPalette.GetBellShimmer(timer);
                Dust d = Dust.NewDustPerfect(playerCenter + Main.rand.NextVector2Circular(18f, 18f),
                    DustID.Enchanted_Gold, Vector2.Zero, 0, shimmer, 0.5f);
                d.noGravity = true;
            }

            // Music notes (sparse)
            if ((int)timer % 30 == 0)
                LaCampanellaVFXLibrary.SpawnMusicNotes(playerCenter, 1, 20f, 0.65f, 0.85f, 35);

            float flicker = Main.rand.NextFloat(0.8f, 1.0f);
            float pulse = 0.25f * flicker;
            Lighting.AddLight(playerCenter, ChimeOrange.ToVector3() * pulse);
        }

        /// <summary>
        /// Bell ring stun proc VFX — golden bell flash with expanding chime ring.
        /// Triggered at 8% chance on any attack.
        /// </summary>
        public static void BellRingStunVFX(Vector2 targetCenter)
        {
            // Golden flare flash
            LaCampanellaVFXLibrary.DrawBloom(targetCenter, 0.4f);

            // Bell chime ring
            CustomParticles.LaCampanellaBellChime(targetCenter, 8);
            LaCampanellaVFXLibrary.SpawnBellChimeRings(targetCenter, 2, 0.3f);

            // Gold sparkle burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(targetCenter, DustID.GoldFlame,
                    vel, 0, ChimeGold, 1.0f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.SpawnMusicNotes(targetCenter, 2, 12f, 0.7f, 0.9f, 25);
            Lighting.AddLight(targetCenter, ChimeGold.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Magic fire trail VFX — fire particles trailing behind magic projectiles.
        /// </summary>
        public static void MagicFireTrailVFX(Vector2 trailPos)
        {
            Vector2 vel = Main.rand.NextVector2Circular(0.5f, 0.5f) + new Vector2(0, -0.3f);
            Color col = Color.Lerp(ChimeOrange, ChimeYellow, Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(trailPos + Main.rand.NextVector2Circular(6f, 6f),
                DustID.Torch, vel, 0, col, 1.0f);
            d.noGravity = true;
        }
    }

    // =============================================================================
    //  INFERNAL VIRTUOSO (Theme Tier 2 Ultimate)
    //  Identity: Intense smoky inferno, heavy orbiting flames, 15% stun+AOE proc.
    // =============================================================================

    /// <summary>
    /// VFX for Infernal Virtuoso — Theme Tier 2 ultimate magic accessory.
    /// +22% magic damage, +10% crit, -12% mana, fire immunity,
    /// 15% bell ring stun with AOE fire explosion.
    /// </summary>
    public static class InfernalVirtuosoVFX
    {
        private static readonly Color VirtuosoOrange = LaCampanellaPalette.InfernalOrange;
        private static readonly Color VirtuosoGold = LaCampanellaPalette.BellGold;
        private static readonly Color VirtuosoFlame = LaCampanellaPalette.FlameYellow;
        private static readonly Color VirtuosoEmber = LaCampanellaPalette.DeepEmber;
        private static readonly Color VirtuosoWhite = LaCampanellaPalette.WhiteHot;

        /// <summary>
        /// Ambient virtuoso aura — the enhanced version of Chime of Flames.
        /// Heavy smoke billowing, intense orbiting flames, rising infernal embers,
        /// bell shimmer bursts. The most visually intense mage aura.
        /// </summary>
        public static void AmbientVirtuosoAura(Vector2 playerCenter, float timer)
        {
            // Heavy smoke billowing (more frequent than Chime)
            if ((int)timer % 5 == 0)
            {
                LaCampanellaVFXLibrary.SpawnHeavySmoke(playerCenter, 1, 0.4f, 1.2f, 35);
            }

            // Intense orbiting flames (3-point, faster)
            if ((int)timer % 4 == 0)
            {
                float baseAngle = timer * 0.04f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 25f + MathF.Sin(timer * 0.08f + i) * 5f;
                    Vector2 flamePos = playerCenter + angle.ToRotationVector2() * radius;
                    Color col = Color.Lerp(VirtuosoEmber, VirtuosoFlame, (float)i / 3f);

                    if (Main.rand.NextBool(2))
                    {
                        Dust d = Dust.NewDustPerfect(flamePos, DustID.Torch,
                            angle.ToRotationVector2() * 0.5f, 0, col, 1.1f);
                        d.noGravity = true;
                    }
                }
            }

            // Rising infernal embers (dense)
            if ((int)timer % 5 == 0)
            {
                Vector2 pos = playerCenter + Main.rand.NextVector2Circular(28f, 28f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1.2f - Main.rand.NextFloat(1f));
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            // Bell shimmer bursts
            if ((int)timer % 12 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust d = Dust.NewDustPerfect(
                        playerCenter + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.Enchanted_Gold, Vector2.Zero, 0, VirtuosoGold * 0.7f, 0.6f);
                    d.noGravity = true;
                }
            }

            // Music notes (more frequent for ultimate)
            if ((int)timer % 18 == 0)
                LaCampanellaVFXLibrary.SpawnMusicNotes(playerCenter, 1, 22f, 0.7f, 0.9f, 30);

            float flicker = Main.rand.NextFloat(0.85f, 1.0f);
            float pulse = 0.35f + MathF.Sin(timer * 0.05f) * 0.1f;
            Lighting.AddLight(playerCenter,
                Color.Lerp(VirtuosoOrange, VirtuosoGold, MathF.Sin(timer * 0.05f) * 0.5f + 0.5f).ToVector3()
                * pulse * flicker);
        }

        /// <summary>
        /// Bell ring stun + AOE VFX — enhanced stun proc with fire explosion.
        /// Triggered at 15% chance. Bigger bell flash, cascading rings,
        /// fire damage explosion on surrounding enemies.
        /// </summary>
        public static void BellRingStunAOEVFX(Vector2 targetCenter)
        {
            // Massive bell flash
            LaCampanellaVFXLibrary.DrawBloom(targetCenter, 0.7f);

            // White flare core
            for (int i = 0; i < 3; i++)
            {
                Dust d = Dust.NewDustPerfect(targetCenter, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(1f, 1f), 0, VirtuosoWhite, 1.2f);
                d.noGravity = true;
            }

            // Bell chime cascade (3 rings)
            for (int i = 0; i < 3; i++)
            {
                CustomParticles.LaCampanellaBellChime(targetCenter, 10 + i * 3);
            }
            LaCampanellaVFXLibrary.SpawnBellChimeRings(targetCenter, 3, 0.35f);

            // Gold sparkle burst (enhanced)
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = Color.Lerp(VirtuosoOrange, VirtuosoGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(targetCenter, DustID.Torch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.SpawnEmberScatter(targetCenter, 6, 3f);
            LaCampanellaVFXLibrary.SpawnMusicNotes(targetCenter, 4, 20f, 0.8f, 1.0f, 30);
            Lighting.AddLight(targetCenter, VirtuosoWhite.ToVector3() * 1.2f);
        }

        /// <summary>
        /// Fire explosion on target VFX — AOE fire damage on nearby enemies
        /// triggered by the Virtuoso's bell ring.
        /// </summary>
        public static void FireExplosionOnTargetVFX(Vector2 enemyCenter)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Color col = LaCampanellaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(enemyCenter, DustID.Torch, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            LaCampanellaVFXLibrary.DrawBloom(enemyCenter, 0.3f);
            Lighting.AddLight(enemyCenter, VirtuosoOrange.ToVector3() * 0.5f);
        }
    }
}
