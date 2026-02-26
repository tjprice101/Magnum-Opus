using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.SandboxLastPrism.Systems;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.MoonlightSonata.Minions
{
    /// <summary>
    /// Overhauled VFX for Goliath of Moonlight — the massive summoner minion.
    /// Theme: Cosmic entity, gravitational presence, devastating beam charge/fire.
    /// Every effect conveys the weight and power of a cosmic entity.
    ///
    /// Visual Identity: Gravitational cosmic entity — a walking singularity.
    /// - GravityWellDust: Spiraling particles drawn into the Goliath's gravitational field
    /// - LunarMote: Crescent fragments orbiting like captured moons
    /// - StarPointDust: Sharp star-core sparks from energy discharge
    /// - ResonantPulseDust: Expanding gravitational shockwaves on impacts
    ///
    /// Conductor Mode Integration:
    ///   When the owner has Conductor Mode active (from Staff of the Lunar Phases),
    ///   the Goliath's VFX are enhanced with cursor-directed energy arcs,
    ///   denser gravitational spirals, and a beam direction indicator during charge.
    ///
    /// Shader Integration:
    ///   GravitationalRift.fx — Spiral vortex overlay during beam charge (rendered in GoliathOfMoonlight.PreDraw)
    ///   SummonCircle.fx     — Sigil overlay beneath Goliath during charge (rendered in GoliathOfMoonlight.PreDraw)
    ///
    /// Unique identity vs other Moonlight content:
    ///   Incisor           = constellation precision (surgical nodes)
    ///   Eternal Moon      = tidal flow (crescent wakes)
    ///   Moonlight's Calling = prismatic refraction (spectral shards)
    ///   Resurrection      = heavy comet impact (burning embers)
    ///   Goliath           = COSMIC GRAVITY (spiraling pull, nebula glow, gravitational shockwaves)
    /// </summary>
    public static class GoliathVFX
    {
        // === UNIQUE COLOR ACCENTS — cosmic entity palette ===
        public static readonly Color CosmicVoid = new Color(20, 8, 40);
        public static readonly Color GravityWell = new Color(100, 60, 180);
        public static readonly Color NebulaPurple = new Color(150, 80, 220);
        public static readonly Color StarCore = new Color(210, 225, 255);
        public static readonly Color EnergyTendril = new Color(180, 140, 255);
        public static readonly Color EventHorizon = new Color(60, 20, 120);

        /// <summary>
        /// Returns a cosmic color that shifts from dark void to bright star-core based on intensity.
        /// </summary>
        public static Color GetCosmicColor(float progress, float chargeLevel)
        {
            float brightShift = MathHelper.Clamp(chargeLevel * 0.15f, 0f, 0.5f);
            Color dark = Color.Lerp(CosmicVoid, GravityWell, progress);
            Color bright = Color.Lerp(NebulaPurple, StarCore, progress);
            return Color.Lerp(dark, bright, brightShift + progress * 0.4f);
        }

        // ═══════════════════════════════════════════
        //  AMBIENT EFFECTS — Always-on cosmic aura
        // ═══════════════════════════════════════════

        /// <summary>
        /// Ambient cosmic aura — orbiting GravityWellDust, LunarMote crescent moons,
        /// StarPointDust star sparks, gravitational shimmer.
        /// Called every frame from Goliath's AI when not attacking.
        /// </summary>
        public static void AmbientAura(Vector2 center, int frameCounter)
        {
            // GravityWellDust — spiraling particles drawn toward the Goliath
            if (frameCounter % 3 == 0)
            {
                float spawnAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float spawnRadius = 35f + Main.rand.NextFloat(20f);
                Vector2 spawnPos = center + spawnAngle.ToRotationVector2() * spawnRadius;
                Color wellColor = Color.Lerp(CosmicVoid, GravityWell, Main.rand.NextFloat(0.7f));
                Dust well = Dust.NewDustPerfect(spawnPos,
                    ModContent.DustType<GravityWellDust>(),
                    Vector2.Zero, 0, wellColor, 0.2f);
                well.customData = new GravityWellBehavior
                {
                    GravityCenter = center,
                    PullStrength = 0.06f,
                    SpiralSpeed = 0.35f,
                    BaseScale = 0.2f,
                    Lifetime = 28,
                    VelocityDecay = 0.97f
                };
            }

            // Orbiting LunarMote — 3 captured crescent moons in slow orbit
            if (frameCounter % 12 == 0)
            {
                float orbitAngle = frameCounter * 0.035f;
                for (int i = 0; i < 3; i++)
                {
                    float moteAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 28f + MathF.Sin(frameCounter * 0.02f + i) * 6f;
                    Color moteColor = Color.Lerp(EnergyTendril, MoonlightVFXLibrary.IceBlue, (float)i / 3f);
                    Dust mote = Dust.NewDustPerfect(center,
                        ModContent.DustType<LunarMote>(),
                        Vector2.Zero, 0, moteColor, 0.25f);
                    mote.customData = new LunarMoteBehavior(center, moteAngle)
                    {
                        OrbitRadius = radius,
                        OrbitSpeed = 0.03f,
                        Lifetime = 30,
                        FadePower = 0.93f
                    };
                }
            }

            // StarPointDust — sharp star-core sparks shed from the entity
            if (frameCounter % 8 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(18f, 18f);
                Color starColor = Color.Lerp(NebulaPurple, StarCore, Main.rand.NextFloat(0.5f));
                Dust star = Dust.NewDustPerfect(center + offset,
                    ModContent.DustType<StarPointDust>(),
                    new Vector2(0, -0.3f), 0, starColor, 0.16f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.06f,
                    TwinkleFrequency = 0.3f,
                    Lifetime = 28,
                    FadeStartTime = 10
                };
            }

            // Sparse music note float (every 30 frames)
            if (frameCounter % 30 == 0)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(center + new Vector2(0f, -20f), 1, 15f, 0.7f, 0.85f, 40);
            }

            // Cosmic glow — pulsing gravitational presence
            float pulse = 0.45f + MathF.Sin(frameCounter * 0.04f) * 0.12f;
            Color lightColor = Color.Lerp(GravityWell, NebulaPurple,
                MathF.Sin(frameCounter * 0.02f) * 0.5f + 0.5f);
            Lighting.AddLight(center, lightColor.ToVector3() * pulse);
        }

        // ═══════════════════════════════════════════
        //  CHARGE EFFECTS — Building devastating beam
        // ═══════════════════════════════════════════

        /// <summary>
        /// Charge buildup VFX — GravityWellDust spiraling inward, ResonantPulseDust
        /// compression rings, StarPointDust energy sparks, LunarMote convergence.
        /// Called every frame during the beam charge sequence.
        /// progress: 0→1 from start to fire.
        /// </summary>
        public static void ChargeBuildup(Vector2 chargeCenter, float progress)
        {
            // GravityWellDust converging spiral — the signature gravitational pull
            float convergeRadius = 80f * (1f - progress * 0.6f);
            int particleCount = (int)(3 + progress * 6);

            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount + Main.GameUpdateCount * 0.05f;
                Vector2 particlePos = chargeCenter + angle.ToRotationVector2() * convergeRadius;
                Color chargeColor = Color.Lerp(GravityWell, StarCore, progress);
                Dust well = Dust.NewDustPerfect(particlePos,
                    ModContent.DustType<GravityWellDust>(),
                    Vector2.Zero, 0, chargeColor, 0.22f + progress * 0.15f);
                well.customData = new GravityWellBehavior
                {
                    GravityCenter = chargeCenter,
                    PullStrength = 0.08f + progress * 0.12f,
                    SpiralSpeed = 0.5f * (1f - progress * 0.3f),
                    BaseScale = 0.22f + progress * 0.15f,
                    Lifetime = (int)(20 + progress * 10),
                    VelocityDecay = 0.96f
                };
            }

            // ResonantPulseDust compression rings — gravity compressing inward
            if (progress > 0.2f && Main.GameUpdateCount % 15 == 0)
            {
                Color compressColor = Color.Lerp(EventHorizon, NebulaPurple, progress);
                Dust pulse = Dust.NewDustPerfect(chargeCenter,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, compressColor,
                    0.18f + progress * 0.1f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.03f + progress * 0.02f,
                    ExpansionDecay = 0.94f,
                    Lifetime = 14 + (int)(progress * 6),
                    PulseFrequency = 0.25f
                };
            }

            // StarPointDust energy arcs — sharp crackling sparks toward center
            if (progress > 0.3f && Main.rand.NextBool(3))
            {
                float arcAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float arcDist = Main.rand.NextFloat(20f, 40f) * (1f - progress * 0.5f);
                Vector2 arcStart = chargeCenter + arcAngle.ToRotationVector2() * arcDist;
                Vector2 toCenter = (chargeCenter - arcStart).SafeNormalize(Vector2.Zero) * 2f;
                Color arcColor = Color.Lerp(EnergyTendril, StarCore, progress);
                Dust star = Dust.NewDustPerfect(arcStart,
                    ModContent.DustType<StarPointDust>(),
                    toCenter, 0, arcColor, 0.2f * (0.5f + progress));
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.2f,
                    TwinkleFrequency = 0.7f,
                    Lifetime = 12,
                    FadeStartTime = 3
                };
            }

            // LunarMote convergence — crescent moons being pulled in (progress > 40%)
            if (progress > 0.4f && Main.rand.NextBool(5))
            {
                float moteAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float moteRadius = 40f * (1f - progress * 0.5f);
                Color moteColor = Color.Lerp(EnergyTendril, MoonlightVFXLibrary.MoonWhite, progress);
                Dust mote = Dust.NewDustPerfect(chargeCenter + moteAngle.ToRotationVector2() * moteRadius,
                    ModContent.DustType<LunarMote>(),
                    Vector2.Zero, 0, moteColor, 0.28f);
                mote.customData = new LunarMoteBehavior(chargeCenter, moteAngle)
                {
                    OrbitRadius = moteRadius,
                    OrbitSpeed = 0.1f + progress * 0.1f,
                    Lifetime = 18,
                    FadePower = 0.9f
                };
            }

            // Growing bloom at center
            float bloomScale = 0.2f + progress * 0.6f;
            CustomParticles.MoonlightFlare(chargeCenter, bloomScale);

            // GodRaySystem pulse at high charge (progress > 70%)
            if (progress > 0.7f && Main.GameUpdateCount % 20 == 0)
            {
                GodRaySystem.CreateBurst(chargeCenter, NebulaPurple, 4, 30f * progress, 12,
                    GodRaySystem.GodRayStyle.Pulsing, MoonlightVFXLibrary.IceBlue);
            }

            // Music notes gathering toward charge point (progress > 50%)
            if (progress > 0.5f && Main.rand.NextBool(4))
            {
                Vector2 noteOffset = Main.rand.NextVector2Circular(30f, 30f);
                MoonlightVFXLibrary.SpawnMusicNotes(chargeCenter + noteOffset, 1, 5f, 0.75f, 0.95f, 20);
            }

            // Lighting intensifies
            Lighting.AddLight(chargeCenter, StarCore.ToVector3() * (0.5f + progress * 0.8f));
        }

        /// <summary>
        /// Charge release flash — the moment the beam fires.
        /// Massive GravityWellDust explosion, ResonantPulseDust gravitational shockwaves,
        /// StarPointDust perpendicular sparks, GodRaySystem + screen effects.
        /// </summary>
        public static void ChargeReleaseFlash(Vector2 firePos, Vector2 beamDirection)
        {
            // Massive central flash cascade
            CustomParticles.GenericFlare(firePos, Color.White, 1.2f, 18);
            CustomParticles.GenericFlare(firePos, StarCore, 1.0f, 22);
            CustomParticles.GenericFlare(firePos, NebulaPurple, 0.8f, 25);
            CustomParticles.GenericFlare(firePos, MoonlightVFXLibrary.DarkPurple, 0.5f, 28);

            // GravityWellDust radial explosion — matter ejected from singularity
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                Color wellColor = GetCosmicColor((float)i / 16f, 1f);
                Dust well = Dust.NewDustPerfect(firePos,
                    ModContent.DustType<GravityWellDust>(),
                    vel, 0, wellColor, 0.35f);
                well.customData = new GravityWellBehavior
                {
                    GravityCenter = Vector2.Zero,
                    PullStrength = 0f,
                    SpiralSpeed = 0f,
                    BaseScale = 0.35f,
                    Lifetime = 28,
                    VelocityDecay = 0.94f
                };
            }

            // ResonantPulseDust gravitational shockwaves — expanding rings
            for (int ring = 0; ring < 4; ring++)
            {
                float progress = ring / 4f;
                Color ringColor = Color.Lerp(EventHorizon, NebulaPurple, progress);
                Dust pulse = Dust.NewDustPerfect(firePos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.3f + ring * 0.08f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.05f + ring * 0.015f,
                    ExpansionDecay = 0.93f,
                    Lifetime = 18 + ring * 4,
                    PulseFrequency = 0.2f
                };
            }

            // StarPointDust perpendicular blast sparks
            Vector2 perp = new Vector2(-beamDirection.Y, beamDirection.X);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int j = 0; j < 5; j++)
                {
                    Vector2 sparkVel = perp * side * Main.rand.NextFloat(3f, 8f)
                        + beamDirection * Main.rand.NextFloat(1f, 3f);
                    Color sparkColor = Color.Lerp(StarCore, EnergyTendril, Main.rand.NextFloat());
                    Dust star = Dust.NewDustPerfect(firePos,
                        ModContent.DustType<StarPointDust>(),
                        sparkVel, 0, sparkColor, 0.25f);
                    star.customData = new StarPointBehavior
                    {
                        RotationSpeed = 0.18f,
                        TwinkleFrequency = 0.5f,
                        Lifetime = 18,
                        FadeStartTime = 5
                    };
                }
            }

            // LunarMote crescent burst — 6 moons ejected
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Color moteColor = Color.Lerp(EnergyTendril, MoonlightVFXLibrary.MoonWhite, (float)i / 6f);
                Dust mote = Dust.NewDustPerfect(firePos,
                    ModContent.DustType<LunarMote>(),
                    angle.ToRotationVector2() * 2f,
                    0, moteColor, 0.35f);
                mote.customData = new LunarMoteBehavior(firePos, angle)
                {
                    OrbitRadius = 25f + i * 5f,
                    OrbitSpeed = 0.06f,
                    Lifetime = 28,
                    FadePower = 0.93f
                };
            }

            // Halo ring cascade
            for (int i = 0; i < 3; i++)
            {
                Color haloColor = Color.Lerp(NebulaPurple, MoonlightVFXLibrary.IceBlue, i / 3f);
                CustomParticles.HaloRing(firePos, haloColor, 0.5f + i * 0.2f, 18 + i * 5);
            }

            // GodRaySystem — massive beam release burst
            GodRaySystem.CreateBurst(firePos, NebulaPurple, 8, 100f, 25,
                GodRaySystem.GodRayStyle.Explosion, StarCore);

            // Screen effects
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(firePos, NebulaPurple, 0.6f, 25);
                MagnumScreenEffects.AddScreenShake(4f);
            }

            // Chromatic aberration flash
            try
            {
                SLPFlashSystem.SetCAFlashEffect(
                    intensity: 0.2f,
                    lifetime: 14,
                    whiteIntensity: 0.5f,
                    distanceMult: 0.35f,
                    moveIn: true);
            }
            catch { }

            // Music note burst
            MoonlightVFXLibrary.SpawnMusicNotes(firePos, 5, 20f, 0.85f, 1.1f, 35);

            Lighting.AddLight(firePos, StarCore.ToVector3() * 2f);
        }

        // ═══════════════════════════════════════════
        //  BEAM EFFECTS — The devastating beam itself
        // ═══════════════════════════════════════════

        /// <summary>
        /// Beam body VFX — GravityWellDust and StarPointDust along active beam,
        /// LunarMote orbiting nodes. Called per frame while beam is firing.
        /// </summary>
        public static void BeamBodyParticles(Vector2 beamStart, Vector2 beamEnd)
        {
            float beamLength = Vector2.Distance(beamStart, beamEnd);
            Vector2 beamDir = (beamEnd - beamStart).SafeNormalize(Vector2.Zero);
            Vector2 perp = new Vector2(-beamDir.Y, beamDir.X);

            // GravityWellDust scattered along beam — nebula wake
            int particlesPerFrame = Math.Max(3, (int)(beamLength / 60f));
            for (int i = 0; i < particlesPerFrame; i++)
            {
                float t = Main.rand.NextFloat();
                Vector2 pos = Vector2.Lerp(beamStart, beamEnd, t);
                pos += perp * Main.rand.NextFloat(-8f, 8f);
                Vector2 driftVel = perp * Main.rand.NextFloat(-1.5f, 1.5f);
                Color beamColor = Color.Lerp(NebulaPurple, EnergyTendril, t);
                Dust well = Dust.NewDustPerfect(pos,
                    ModContent.DustType<GravityWellDust>(),
                    driftVel, 0, beamColor, 0.2f);
                well.customData = new GravityWellBehavior
                {
                    GravityCenter = Vector2.Zero,
                    PullStrength = 0f,
                    SpiralSpeed = 0f,
                    BaseScale = 0.2f,
                    Lifetime = 16,
                    VelocityDecay = 0.95f
                };
            }

            // StarPointDust sparks along beam (sparse)
            if (Main.rand.NextBool(3))
            {
                float sparkT = Main.rand.NextFloat();
                Vector2 sparkPos = Vector2.Lerp(beamStart, beamEnd, sparkT);
                sparkPos += perp * Main.rand.NextFloat(-5f, 5f);
                Color sparkColor = Color.Lerp(StarCore, EnergyTendril, Main.rand.NextFloat());
                Dust star = Dust.NewDustPerfect(sparkPos,
                    ModContent.DustType<StarPointDust>(),
                    perp * Main.rand.NextFloat(-1f, 1f), 0, sparkColor, 0.18f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.12f,
                    TwinkleFrequency = 0.6f,
                    Lifetime = 14,
                    FadeStartTime = 4
                };
            }

            // LunarMote orbiting nodes along beam (every 8 frames)
            if (Main.rand.NextBool(8))
            {
                float noteT = Main.rand.NextFloat(0.1f, 0.9f);
                Vector2 notePos = Vector2.Lerp(beamStart, beamEnd, noteT);
                float orbitAngle = Main.GameUpdateCount * 0.1f;
                Color moteColor = Color.Lerp(EnergyTendril, MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust mote = Dust.NewDustPerfect(notePos,
                    ModContent.DustType<LunarMote>(),
                    Vector2.Zero, 0, moteColor, 0.22f);
                mote.customData = new LunarMoteBehavior(notePos, orbitAngle)
                {
                    OrbitRadius = 8f,
                    OrbitSpeed = 0.12f,
                    Lifetime = 16,
                    FadePower = 0.9f
                };
            }

            // Music notes along beam (sparse)
            if (Main.rand.NextBool(8))
            {
                float noteDist = Main.rand.NextFloat(100f, beamLength * 0.8f);
                Vector2 notePos = beamStart + beamDir * noteDist;
                MoonlightVFXLibrary.SpawnMusicNotes(notePos, 1, 8f, 0.75f, 0.9f, 25);
            }

            // Lighting along beam
            int lightPoints = Math.Max(2, (int)(beamLength / 100f));
            for (int i = 0; i < lightPoints; i++)
            {
                float t = (float)(i + 1) / (lightPoints + 1);
                Vector2 lightPos = Vector2.Lerp(beamStart, beamEnd, t);
                Lighting.AddLight(lightPos, NebulaPurple.ToVector3() * 0.8f);
            }
        }

        /// <summary>
        /// Beam impact point VFX — StarPointDust sparks and ResonantPulseDust ring.
        /// </summary>
        public static void BeamImpactPoint(Vector2 impactPos)
        {
            // StarPointDust impact sparks
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkColor = Color.Lerp(StarCore, NebulaPurple, Main.rand.NextFloat());
                Dust star = Dust.NewDustPerfect(impactPos,
                    ModContent.DustType<StarPointDust>(),
                    sparkVel, 0, sparkColor, 0.2f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.15f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 14,
                    FadeStartTime = 4
                };
            }

            // ResonantPulseDust impact ring
            if (Main.rand.NextBool(3))
            {
                Dust pulse = Dust.NewDustPerfect(impactPos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, NebulaPurple, 0.15f);
                pulse.customData = new ResonantPulseBehavior(0.025f, 12);
            }

            // Impact glow
            if (Main.rand.NextBool(2))
            {
                CustomParticles.MoonlightFlare(impactPos, 0.3f);
            }

            Lighting.AddLight(impactPos, StarCore.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Beam ricochet explosion — massive detonation using custom dusts.
        /// GravityWellDust radial burst + ResonantPulseDust shockwaves +
        /// StarPointDust shrapnel + LunarMote orbit.
        /// </summary>
        public static void BeamExplosion(Vector2 position)
        {
            // Goliath-scale explosion using custom particles
            CustomParticles.MoonlightCrescendo(position, 1.8f);
            CustomParticles.ExplosionBurst(position, NebulaPurple, 20, 9f);

            // Flash cascade
            CustomParticles.GenericFlare(position, Color.White, 0.8f, 16);
            CustomParticles.GenericFlare(position, StarCore, 0.6f, 20);
            CustomParticles.GenericFlare(position, NebulaPurple, 0.5f, 22);

            // GravityWellDust radial burst — cosmic matter ejected
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                Color wellColor = GetCosmicColor((float)i / 12f, 0.6f);
                Dust well = Dust.NewDustPerfect(position,
                    ModContent.DustType<GravityWellDust>(),
                    vel, 0, wellColor, 0.3f);
                well.customData = new GravityWellBehavior
                {
                    GravityCenter = Vector2.Zero,
                    PullStrength = 0f,
                    SpiralSpeed = 0f,
                    BaseScale = 0.3f,
                    Lifetime = 24,
                    VelocityDecay = 0.94f
                };
            }

            // ResonantPulseDust shockwaves
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = Color.Lerp(EventHorizon, NebulaPurple, i / 3f);
                Dust pulse = Dust.NewDustPerfect(position,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.25f + i * 0.06f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.04f + i * 0.012f,
                    ExpansionDecay = 0.94f,
                    Lifetime = 16 + i * 3,
                    PulseFrequency = 0.25f
                };
            }

            // StarPointDust shrapnel burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color starColor = Color.Lerp(StarCore, EnergyTendril, Main.rand.NextFloat());
                Dust star = Dust.NewDustPerfect(position,
                    ModContent.DustType<StarPointDust>(),
                    vel, 0, starColor, 0.22f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.15f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 18,
                    FadeStartTime = 5
                };
            }

            // Halo rings
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = Color.Lerp(NebulaPurple, MoonlightVFXLibrary.IceBlue, i / 3f);
                CustomParticles.HaloRing(position, ringColor, 0.4f + i * 0.15f, 16 + i * 4);
            }

            // Music notes
            MoonlightVFXLibrary.SpawnMusicNotes(position, 3, 20f, 0.8f, 1.0f, 30);

            Lighting.AddLight(position, StarCore.ToVector3() * 1.2f);
        }

        /// <summary>
        /// Ricochet line between two enemies — lightning chain visual.
        /// </summary>
        public static void BeamRicochetLine(Vector2 start, Vector2 end)
        {
            MagnumVFX.DrawMoonlightLightning(start, end, 5, 15f, 2, 0.3f);
        }

        // ═══════════════════════════════════════════
        //  MELEE COMBAT EFFECTS
        // ═══════════════════════════════════════════

        /// <summary>
        /// Goliath melee hit VFX — heavy cosmic impact with custom dusts.
        /// ResonantPulseDust gravitational shockwave + GravityWellDust burst +
        /// StarPointDust shrapnel.
        /// </summary>
        public static void MeleeHitImpact(Vector2 hitPos)
        {
            // Central flash
            CustomParticles.GenericFlare(hitPos, Color.White, 0.6f, 15);
            CustomParticles.GenericFlare(hitPos, NebulaPurple, 0.5f, 18);

            // Gradient halo cascade
            MoonlightVFXLibrary.SpawnGradientHaloRings(hitPos, 4, 0.3f);

            // ResonantPulseDust gravitational shockwave
            Dust pulse = Dust.NewDustPerfect(hitPos,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, GravityWell, 0.25f);
            pulse.customData = new ResonantPulseBehavior
            {
                ExpansionRate = 0.045f,
                ExpansionDecay = 0.93f,
                Lifetime = 16,
                PulseFrequency = 0.3f
            };

            // GravityWellDust burst — cosmic debris from impact
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Color wellColor = Color.Lerp(GravityWell, NebulaPurple, Main.rand.NextFloat());
                Dust well = Dust.NewDustPerfect(hitPos,
                    ModContent.DustType<GravityWellDust>(),
                    vel, 0, wellColor, 0.22f);
                well.customData = new GravityWellBehavior
                {
                    GravityCenter = Vector2.Zero,
                    PullStrength = 0f,
                    BaseScale = 0.22f,
                    Lifetime = 20,
                    VelocityDecay = 0.94f
                };
            }

            // StarPointDust shrapnel
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Color starColor = Color.Lerp(StarCore, EnergyTendril, Main.rand.NextFloat());
                Dust star = Dust.NewDustPerfect(hitPos,
                    ModContent.DustType<StarPointDust>(),
                    vel, 0, starColor, 0.18f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.12f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 16,
                    FadeStartTime = 5
                };
            }

            // Music notes
            MoonlightVFXLibrary.SpawnMusicNotes(hitPos, 3, 20f, 0.75f, 0.95f, 30);

            Lighting.AddLight(hitPos, NebulaPurple.ToVector3() * 1.0f);
        }

        // ═══════════════════════════════════════════
        //  MOVEMENT EFFECTS
        // ═══════════════════════════════════════════

        /// <summary>
        /// Jump particles — GravityWellDust and ResonantPulseDust kick-up.
        /// </summary>
        public static void JumpEffect(Vector2 feetPos)
        {
            // GravityWellDust ground burst
            for (int i = 0; i < 5; i++)
            {
                Vector2 dustVel = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-2f, 0.5f));
                Color jumpColor = Color.Lerp(CosmicVoid, GravityWell, Main.rand.NextFloat());
                Dust well = Dust.NewDustPerfect(
                    feetPos + new Vector2(Main.rand.NextFloat(-10f, 10f), 0f),
                    ModContent.DustType<GravityWellDust>(),
                    dustVel, 0, jumpColor, 0.18f);
                well.customData = new GravityWellBehavior
                {
                    GravityCenter = Vector2.Zero,
                    PullStrength = 0f,
                    BaseScale = 0.18f,
                    Lifetime = 18,
                    VelocityDecay = 0.95f
                };
            }

            // Small ResonantPulseDust ring
            Dust jumpPulse = Dust.NewDustPerfect(feetPos,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, GravityWell, 0.15f);
            jumpPulse.customData = new ResonantPulseBehavior(0.03f, 12);
        }

        /// <summary>
        /// Landing impact — GravityWellDust burst + ResonantPulseDust shockwave.
        /// </summary>
        public static void LandingEffect(Vector2 feetPos)
        {
            // GravityWellDust ground scatter
            for (int i = 0; i < 8; i++)
            {
                Vector2 dustVel = new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-3f, -0.5f));
                Color landColor = Color.Lerp(GravityWell, NebulaPurple, Main.rand.NextFloat());
                Dust well = Dust.NewDustPerfect(
                    feetPos + new Vector2(Main.rand.NextFloat(-12f, 12f), 0f),
                    ModContent.DustType<GravityWellDust>(),
                    dustVel, 0, landColor, 0.2f);
                well.customData = new GravityWellBehavior
                {
                    GravityCenter = Vector2.Zero,
                    PullStrength = 0f,
                    BaseScale = 0.2f,
                    Lifetime = 20,
                    VelocityDecay = 0.94f
                };
            }

            // ResonantPulseDust landing shockwave
            Dust landPulse = Dust.NewDustPerfect(feetPos,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, GravityWell, 0.2f);
            landPulse.customData = new ResonantPulseBehavior(0.035f, 14);

            CustomParticles.MoonlightHalo(feetPos, 0.25f);
        }

        /// <summary>
        /// Teleport VFX — GravityWellDust implosion-explosion, ResonantPulseDust ring,
        /// LunarMote burst.
        /// </summary>
        public static void TeleportFlash(Vector2 position)
        {
            CustomParticles.GenericFlare(position, Color.White, 0.7f, 16);
            CustomParticles.GenericFlare(position, NebulaPurple, 0.5f, 20);

            // GravityWellDust teleport burst — matter displaced
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color tpColor = Color.Lerp(GravityWell, EnergyTendril, Main.rand.NextFloat());
                Dust well = Dust.NewDustPerfect(position,
                    ModContent.DustType<GravityWellDust>(),
                    vel, 0, tpColor, 0.22f);
                well.customData = new GravityWellBehavior
                {
                    GravityCenter = Vector2.Zero,
                    PullStrength = 0f,
                    BaseScale = 0.22f,
                    Lifetime = 20,
                    VelocityDecay = 0.95f
                };
            }

            // LunarMote burst — 4 crescents
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Color moteColor = Color.Lerp(EnergyTendril, MoonlightVFXLibrary.MoonWhite, (float)i / 4f);
                Dust mote = Dust.NewDustPerfect(position,
                    ModContent.DustType<LunarMote>(),
                    angle.ToRotationVector2() * 1.5f,
                    0, moteColor, 0.25f);
                mote.customData = new LunarMoteBehavior(position, angle)
                {
                    OrbitRadius = 15f,
                    OrbitSpeed = 0.08f,
                    Lifetime = 22,
                    FadePower = 0.92f
                };
            }

            // ResonantPulseDust teleport ring
            Dust tpPulse = Dust.NewDustPerfect(position,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, NebulaPurple, 0.22f);
            tpPulse.customData = new ResonantPulseBehavior(0.04f, 16);

            CustomParticles.HaloRing(position, NebulaPurple, 0.4f, 18);
            MoonlightVFXLibrary.SpawnMusicNotes(position, 2, 15f, 0.75f, 0.9f, 30);
        }

        // ═══════════════════════════════════════════
        //  DRAW HELPERS
        // ═══════════════════════════════════════════

        /// <summary>
        /// PreDraw bloom for the Goliath — cosmic aura glow around the entity.
        /// Uses {A=0} premultiplied alpha trick — NO SpriteBatch restart needed.
        /// 5-layer bloom with cosmic entity palette.
        /// </summary>
        public static void DrawCosmicBloom(SpriteBatch sb, Vector2 goliathCenter, bool isCharging, float chargeProgress)
        {
            if (sb == null) return;

            Vector2 drawPos = goliathCenter - Main.screenPosition;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.08f;
            float chargeBoost = isCharging ? (1f + chargeProgress * 0.5f) : 1f;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            // Layer 1: Outer cosmic void aura (massive)
            sb.Draw(bloomTex, drawPos, null,
                (CosmicVoid with { A = 0 }) * 0.15f,
                0f, origin, 1.8f * pulse * chargeBoost, SpriteEffects.None, 0f);

            // Layer 2: Event horizon ring
            sb.Draw(bloomTex, drawPos, null,
                (EventHorizon with { A = 0 }) * 0.20f * chargeBoost,
                0f, origin, 1.4f * pulse * chargeBoost, SpriteEffects.None, 0f);

            // Layer 3: Mid nebula purple
            sb.Draw(bloomTex, drawPos, null,
                (NebulaPurple with { A = 0 }) * 0.25f * chargeBoost,
                0f, origin, 1.0f * pulse * chargeBoost, SpriteEffects.None, 0f);

            // Layer 4: Inner gravity well
            sb.Draw(bloomTex, drawPos, null,
                (GravityWell with { A = 0 }) * 0.35f * chargeBoost,
                0f, origin, 0.6f * pulse * chargeBoost, SpriteEffects.None, 0f);

            // Layer 5: Star core (grows during charge)
            float coreAlpha = isCharging ? 0.5f * chargeProgress : 0.15f;
            float coreScale = isCharging ? 0.35f * chargeProgress : 0.2f;
            sb.Draw(bloomTex, drawPos, null,
                (StarCore with { A = 0 }) * coreAlpha,
                0f, origin, coreScale * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draw the beam body with proper bloom rendering.
        /// 4-layer bloom nodes along beam using cosmic entity palette.
        /// </summary>
        public static void DrawBeamBody(SpriteBatch sb, Vector2 beamStart, Vector2 beamEnd, float widthProgress)
        {
            if (sb == null) return;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            float beamLength = Vector2.Distance(beamStart, beamEnd);
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.06f;

            // Draw bloom nodes along the beam
            int nodeCount = Math.Max(3, (int)(beamLength / 30f));
            for (int i = 0; i <= nodeCount; i++)
            {
                float t = (float)i / nodeCount;
                Vector2 nodeWorldPos = Vector2.Lerp(beamStart, beamEnd, t);
                Vector2 nodeDrawPos = nodeWorldPos - Main.screenPosition;

                float beamWidth = widthProgress * pulse;

                // Color shifts along beam length
                Color nodeColor = Color.Lerp(NebulaPurple, EnergyTendril, t);

                // Outer nebula glow
                sb.Draw(bloomTex, nodeDrawPos, null,
                    (nodeColor with { A = 0 }) * 0.20f,
                    0f, origin, 0.6f * beamWidth, SpriteEffects.None, 0f);

                // Mid gravity well
                sb.Draw(bloomTex, nodeDrawPos, null,
                    (GravityWell with { A = 0 }) * 0.35f,
                    0f, origin, 0.4f * beamWidth, SpriteEffects.None, 0f);

                // Inner energy tendril
                sb.Draw(bloomTex, nodeDrawPos, null,
                    (EnergyTendril with { A = 0 }) * 0.50f,
                    0f, origin, 0.25f * beamWidth, SpriteEffects.None, 0f);

                // White-hot core
                sb.Draw(bloomTex, nodeDrawPos, null,
                    (Color.White with { A = 0 }) * 0.65f,
                    0f, origin, 0.12f * beamWidth, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Small beam projectile bloom — for GoliathMoonlightBeam body rendering.
        /// 5-layer bloom with cosmic palette.
        /// </summary>
        public static void DrawSmallBeamBloom(SpriteBatch sb, Vector2 projWorldPos, int ricochetCount)
        {
            if (sb == null) return;

            Vector2 drawPos = projWorldPos - Main.screenPosition;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.08f;
            float bounceScale = 1f + ricochetCount * 0.1f;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            // Layer 1: Outer cosmic void
            sb.Draw(bloomTex, drawPos, null,
                (CosmicVoid with { A = 0 }) * 0.18f,
                0f, origin, 0.6f * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 2: Gravity well purple
            sb.Draw(bloomTex, drawPos, null,
                (GravityWell with { A = 0 }) * 0.28f,
                0f, origin, 0.45f * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 3: Nebula purple
            sb.Draw(bloomTex, drawPos, null,
                (NebulaPurple with { A = 0 }) * 0.40f,
                0f, origin, 0.3f * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 4: Energy tendril
            sb.Draw(bloomTex, drawPos, null,
                (EnergyTendril with { A = 0 }) * 0.55f,
                0f, origin, 0.18f * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 5: White core
            sb.Draw(bloomTex, drawPos, null,
                (Color.White with { A = 0 }) * 0.70f,
                0f, origin, 0.08f * bounceScale * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Small beam hit explosion — GravityWellDust burst, StarPointDust shrapnel,
        /// ResonantPulseDust shockwave, fractal flare pattern.
        /// </summary>
        public static void SmallBeamHitExplosion(Vector2 hitPos, int ricochetCount)
        {
            float intensity = 0.7f + ricochetCount * 0.08f;

            // Central flash
            CustomParticles.GenericFlare(hitPos, Color.White, 0.5f * intensity, 14);
            CustomParticles.GenericFlare(hitPos, NebulaPurple, 0.4f * intensity, 16);

            // Fractal flare burst (signature Moonlight pattern)
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = angle.ToRotationVector2() * 30f;
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(GravityWell, EnergyTendril, progress);
                CustomParticles.GenericFlare(hitPos + flareOffset, fractalColor, 0.35f, 16);
            }

            // GravityWellDust burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color wellColor = Color.Lerp(GravityWell, NebulaPurple, Main.rand.NextFloat());
                Dust well = Dust.NewDustPerfect(hitPos,
                    ModContent.DustType<GravityWellDust>(),
                    vel, 0, wellColor, 0.22f);
                well.customData = new GravityWellBehavior
                {
                    GravityCenter = Vector2.Zero,
                    PullStrength = 0f,
                    BaseScale = 0.22f,
                    Lifetime = 20,
                    VelocityDecay = 0.94f
                };
            }

            // StarPointDust shrapnel
            for (int i = 0; i < 4 + ricochetCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Color starColor = Color.Lerp(StarCore, EnergyTendril, Main.rand.NextFloat());
                Dust star = Dust.NewDustPerfect(hitPos,
                    ModContent.DustType<StarPointDust>(),
                    vel, 0, starColor, 0.18f * intensity);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.15f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 16,
                    FadeStartTime = 4
                };
            }

            // ResonantPulseDust impact shockwave
            Dust hitPulse = Dust.NewDustPerfect(hitPos,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, GravityWell, 0.2f);
            hitPulse.customData = new ResonantPulseBehavior(0.035f, 14);

            // Halo rings
            CustomParticles.HaloRing(hitPos, NebulaPurple, 0.3f, 16);
            CustomParticles.HaloRing(hitPos, EnergyTendril, 0.25f, 20);

            // Music notes
            MoonlightVFXLibrary.SpawnMusicNotes(hitPos, 2, 15f, 0.75f, 0.9f, 28);

            Lighting.AddLight(hitPos, NebulaPurple.ToVector3() * intensity);
        }

        /// <summary>
        /// Small beam death VFX — GravityWellDust scatter, ResonantPulseDust ring,
        /// LunarMote fade.
        /// </summary>
        public static void SmallBeamDeath(Vector2 deathPos)
        {
            MoonlightVFXLibrary.ProjectileImpact(deathPos, 0.8f);

            // GravityWellDust death scatter
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color wellColor = Color.Lerp(GravityWell, NebulaPurple, Main.rand.NextFloat());
                Dust well = Dust.NewDustPerfect(deathPos,
                    ModContent.DustType<GravityWellDust>(),
                    vel, 0, wellColor, 0.2f);
                well.customData = new GravityWellBehavior
                {
                    GravityCenter = Vector2.Zero,
                    PullStrength = 0f,
                    BaseScale = 0.2f,
                    Lifetime = 18,
                    VelocityDecay = 0.95f
                };
            }

            // ResonantPulseDust death ring
            Dust deathPulse = Dust.NewDustPerfect(deathPos,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, GravityWell, 0.18f);
            deathPulse.customData = new ResonantPulseBehavior(0.03f, 14);

            // LunarMote farewell crescent
            for (int i = 0; i < 2; i++)
            {
                float angle = MathHelper.Pi * i;
                Color moteColor = Color.Lerp(EnergyTendril, MoonlightVFXLibrary.MoonWhite, (float)i / 2f);
                Dust mote = Dust.NewDustPerfect(deathPos,
                    ModContent.DustType<LunarMote>(),
                    angle.ToRotationVector2() * 1f,
                    0, moteColor, 0.2f);
                mote.customData = new LunarMoteBehavior(deathPos, angle)
                {
                    OrbitRadius = 10f,
                    OrbitSpeed = 0.06f,
                    Lifetime = 18,
                    FadePower = 0.9f
                };
            }

            // Halo ring cascade
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = Color.Lerp(GravityWell, MoonlightVFXLibrary.MoonWhite, i / 3f);
                CustomParticles.HaloRing(deathPos, ringColor, 0.25f + i * 0.1f, 14 + i * 4);
            }

            MoonlightVFXLibrary.SpawnMusicNotes(deathPos, 3, 20f, 0.8f, 1.0f, 30);
        }

        /// <summary>
        /// Cosmic gravitational light — pulsing between deep void and nebula glow.
        /// </summary>
        public static void AddCosmicLight(Vector2 worldPos, float intensity = 0.5f)
        {
            float pulse = 0.85f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.15f;
            Color lightColor = Color.Lerp(GravityWell, NebulaPurple,
                MathF.Sin(Main.GlobalTimeWrappedHourly * 2.5f) * 0.5f + 0.5f);
            Lighting.AddLight(worldPos, lightColor.ToVector3() * intensity * pulse);
        }

        // ═══════════════════════════════════════════
        //  CONDUCTOR MODE EFFECTS
        // ═══════════════════════════════════════════

        /// <summary>
        /// Enhanced ambient aura when owner has Conductor Mode active.
        /// Denser gravitational spirals, cursor-directed energy arcs from the Goliath
        /// toward the cursor position, and brighter pulsing glow.
        /// Called every frame from Goliath's AI when Conductor Mode is on.
        /// </summary>
        public static void ConductorModeAmbient(Vector2 center, int frameCounter, Vector2 cursorWorldPos)
        {
            // Denser GravityWellDust spiral — every 2 frames instead of 3
            if (frameCounter % 2 == 0)
            {
                float spawnAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float spawnRadius = 40f + Main.rand.NextFloat(25f);
                Vector2 spawnPos = center + spawnAngle.ToRotationVector2() * spawnRadius;
                Color wellColor = Color.Lerp(GravityWell, EnergyTendril, Main.rand.NextFloat(0.6f));
                Dust well = Dust.NewDustPerfect(spawnPos,
                    ModContent.DustType<GravityWellDust>(),
                    Vector2.Zero, 0, wellColor, 0.24f);
                well.customData = new GravityWellBehavior
                {
                    GravityCenter = center,
                    PullStrength = 0.08f,
                    SpiralSpeed = 0.45f,
                    BaseScale = 0.24f,
                    Lifetime = 32,
                    VelocityDecay = 0.96f
                };
            }

            // Cursor-directed StarPointDust energy arcs — sparks flying toward cursor
            if (frameCounter % 4 == 0)
            {
                Vector2 toCursor = (cursorWorldPos - center).SafeNormalize(Vector2.UnitY);
                float arcAngle = toCursor.ToRotation() + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 arcVel = arcAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color arcColor = Color.Lerp(StarCore, EnergyTendril, Main.rand.NextFloat(0.5f));
                Dust star = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(10f, 10f),
                    ModContent.DustType<StarPointDust>(),
                    arcVel, 0, arcColor, 0.2f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.1f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 20,
                    FadeStartTime = 8
                };
            }

            // Extra LunarMote — 4 orbiting crescents instead of 3
            if (frameCounter % 10 == 0)
            {
                float orbitAngle = frameCounter * 0.04f;
                for (int i = 0; i < 4; i++)
                {
                    float moteAngle = orbitAngle + MathHelper.TwoPi * i / 4f;
                    float radius = 32f + MathF.Sin(frameCounter * 0.025f + i) * 7f;
                    Color moteColor = Color.Lerp(EnergyTendril, StarCore, (float)i / 4f);
                    Dust mote = Dust.NewDustPerfect(center,
                        ModContent.DustType<LunarMote>(),
                        Vector2.Zero, 0, moteColor, 0.28f);
                    mote.customData = new LunarMoteBehavior(center, moteAngle)
                    {
                        OrbitRadius = radius,
                        OrbitSpeed = 0.04f,
                        Lifetime = 32,
                        FadePower = 0.93f
                    };
                }
            }

            // Conductor pulse ring — rhythmic gravitational pulses
            if (frameCounter % 25 == 0)
            {
                Color pulseColor = Color.Lerp(GravityWell, NebulaPurple, 0.5f);
                Dust pulse = Dust.NewDustPerfect(center,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, pulseColor, 0.18f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.03f,
                    ExpansionDecay = 0.94f,
                    Lifetime = 16,
                    PulseFrequency = 0.25f
                };
            }

            // Enhanced pulsing cosmic glow
            float conductorPulse = 0.55f + MathF.Sin(frameCounter * 0.06f) * 0.18f;
            Color conductorLight = Color.Lerp(GravityWell, StarCore,
                MathF.Sin(frameCounter * 0.03f) * 0.5f + 0.5f);
            Lighting.AddLight(center, conductorLight.ToVector3() * conductorPulse);
        }

        /// <summary>
        /// Conductor Mode charge buildup — enhanced version of ChargeBuildup with
        /// cursor-directed convergence and a visible beam direction indicator.
        /// Called every frame during beam charge when Conductor Mode is active.
        /// </summary>
        public static void ConductorChargeBuildup(Vector2 chargeCenter, float progress, Vector2 cursorWorldPos)
        {
            // Base charge effects (delegates to standard charge buildup)
            ChargeBuildup(chargeCenter, progress);

            // Additional cursor-directed GravityWellDust converging from cursor direction
            if (progress > 0.15f)
            {
                Vector2 toCursor = (cursorWorldPos - chargeCenter).SafeNormalize(Vector2.UnitY);
                int extraCount = (int)(2 + progress * 4);
                for (int i = 0; i < extraCount; i++)
                {
                    float spread = MathHelper.Lerp(0.8f, 0.3f, progress);
                    float angle = toCursor.ToRotation() + Main.rand.NextFloat(-spread, spread);
                    float dist = 50f * (1f - progress * 0.4f) + Main.rand.NextFloat(20f);
                    Vector2 spawnPos = chargeCenter + angle.ToRotationVector2() * dist;
                    Color wellColor = Color.Lerp(EnergyTendril, StarCore, progress);
                    Dust well = Dust.NewDustPerfect(spawnPos,
                        ModContent.DustType<GravityWellDust>(),
                        Vector2.Zero, 0, wellColor, 0.2f + progress * 0.12f);
                    well.customData = new GravityWellBehavior
                    {
                        GravityCenter = chargeCenter,
                        PullStrength = 0.1f + progress * 0.15f,
                        SpiralSpeed = 0.4f * (1f - progress * 0.2f),
                        BaseScale = 0.2f + progress * 0.12f,
                        Lifetime = (int)(16 + progress * 8),
                        VelocityDecay = 0.95f
                    };
                }
            }

            // Beam direction indicator — StarPointDust line from center toward cursor
            if (progress > 0.3f && Main.GameUpdateCount % 3 == 0)
            {
                Vector2 toCursor = (cursorWorldPos - chargeCenter).SafeNormalize(Vector2.UnitY);
                float indicatorLength = 30f + progress * 40f;
                int indicatorCount = (int)(2 + progress * 3);
                for (int i = 0; i < indicatorCount; i++)
                {
                    float t = (float)(i + 1) / (indicatorCount + 1);
                    Vector2 indicatorPos = chargeCenter + toCursor * (indicatorLength * t);
                    indicatorPos += new Vector2(-toCursor.Y, toCursor.X) * Main.rand.NextFloat(-3f, 3f);
                    Color indicatorColor = Color.Lerp(EnergyTendril, StarCore, t * progress);
                    Dust star = Dust.NewDustPerfect(indicatorPos,
                        ModContent.DustType<StarPointDust>(),
                        toCursor * Main.rand.NextFloat(0.5f, 2f),
                        0, indicatorColor, 0.16f + progress * 0.1f);
                    star.customData = new StarPointBehavior
                    {
                        RotationSpeed = 0.1f,
                        TwinkleFrequency = 0.6f,
                        Lifetime = 10,
                        FadeStartTime = 3
                    };
                }
            }

            // Additional LunarMote convergence toward cursor direction
            if (progress > 0.5f && Main.rand.NextBool(4))
            {
                Vector2 toCursor = (cursorWorldPos - chargeCenter).SafeNormalize(Vector2.UnitY);
                float moteAngle = toCursor.ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);
                float moteRadius = 35f * (1f - progress * 0.4f);
                Color moteColor = Color.Lerp(EnergyTendril, StarCore, progress);
                Dust mote = Dust.NewDustPerfect(
                    chargeCenter + moteAngle.ToRotationVector2() * moteRadius,
                    ModContent.DustType<LunarMote>(),
                    Vector2.Zero, 0, moteColor, 0.3f);
                mote.customData = new LunarMoteBehavior(chargeCenter, moteAngle)
                {
                    OrbitRadius = moteRadius,
                    OrbitSpeed = 0.08f + progress * 0.08f,
                    Lifetime = 16,
                    FadePower = 0.9f
                };
            }
        }

        /// <summary>
        /// Draws a beam direction indicator line from the Goliath toward the target position.
        /// Rendered as a dotted bloom line in the cosmic entity palette.
        /// Uses {A=0} premultiplied alpha — NO SpriteBatch restart needed.
        /// </summary>
        public static void DrawConductorTargetLine(SpriteBatch sb, Vector2 goliathCenter, Vector2 targetPos, float chargeProgress)
        {
            if (sb == null || chargeProgress < 0.2f) return;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            Vector2 toTarget = (targetPos - goliathCenter).SafeNormalize(Vector2.UnitY);
            float maxLength = MathHelper.Clamp(Vector2.Distance(goliathCenter, targetPos), 0f, 200f);
            float lineLength = maxLength * chargeProgress;

            float linePulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 10f) * 0.1f;
            float fadeAlpha = MathHelper.Clamp((chargeProgress - 0.2f) / 0.3f, 0f, 1f);

            // Draw dotted bloom nodes along the indicator line
            int nodeCount = Math.Max(3, (int)(lineLength / 18f));
            for (int i = 0; i <= nodeCount; i++)
            {
                float t = (float)i / nodeCount;
                Vector2 nodeWorldPos = goliathCenter + toTarget * (lineLength * t + 20f);
                Vector2 nodeDrawPos = nodeWorldPos - Main.screenPosition;

                float nodeFade = (1f - t * 0.5f) * fadeAlpha;
                float nodeScale = (0.15f + chargeProgress * 0.1f) * linePulse * (1f - t * 0.3f);

                // Outer energy tendril glow
                sb.Draw(bloomTex, nodeDrawPos, null,
                    (EnergyTendril with { A = 0 }) * 0.3f * nodeFade,
                    0f, origin, nodeScale * 1.5f, SpriteEffects.None, 0f);

                // Inner star core
                sb.Draw(bloomTex, nodeDrawPos, null,
                    (StarCore with { A = 0 }) * 0.5f * nodeFade,
                    0f, origin, nodeScale * 0.8f, SpriteEffects.None, 0f);
            }

            // Endpoint indicator — bright dot at the target end
            if (chargeProgress > 0.5f)
            {
                Vector2 endWorldPos = goliathCenter + toTarget * (lineLength + 20f);
                Vector2 endDrawPos = endWorldPos - Main.screenPosition;
                float endAlpha = MathHelper.Clamp((chargeProgress - 0.5f) / 0.3f, 0f, 1f) * fadeAlpha;

                sb.Draw(bloomTex, endDrawPos, null,
                    (NebulaPurple with { A = 0 }) * 0.35f * endAlpha,
                    0f, origin, 0.4f * linePulse, SpriteEffects.None, 0f);

                sb.Draw(bloomTex, endDrawPos, null,
                    (StarCore with { A = 0 }) * 0.6f * endAlpha,
                    0f, origin, 0.2f * linePulse, SpriteEffects.None, 0f);
            }
        }
    }
}
