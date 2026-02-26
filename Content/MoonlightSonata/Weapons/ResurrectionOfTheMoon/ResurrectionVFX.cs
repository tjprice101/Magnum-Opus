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
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.SandboxLastPrism.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon
{
    /// <summary>
    /// Overhauled VFX for Resurrection of the Moon — "The Final Movement".
    ///
    /// Visual Identity: Heavy astronomical impact — every shot is a falling star.
    /// - CometEmberDust: Burning comet fragments that cool from gold-white to deep violet
    /// - Crater detonations: ResonantPulseDust expanding shockwaves on every impact
    /// - Moonrise flash: StarPointDust and LunarMote scattered on ricochets
    /// - Escalating drama: Each ricochet burns hotter and brighter than the last
    /// - Grand finale: When all ricochets are spent, supernova-class detonation
    ///
    /// Shader Integration (GPU-rendered overlays):
    ///   CometTrail.fx     — Burning ember tail via DrawCometShaderGlow (all projectiles)
    ///   SupernovaBlast.fx — Radial crater explosion via DrawSupernovaBlastOverlay (death/crit)
    ///
    /// Chamber Mechanic VFX:
    ///   Chamber 0 (Standard)  — ResurrectionProjectile, gold-violet comet trail
    ///   Chamber 1 (Comet Core) — CometCore, white-hot penetrating ember wake
    ///   Chamber 2 (Supernova)  — SupernovaShell, heavy artillery with massive AoE
    ///
    /// Unique vs other Moonlight weapons:
    ///   EternalMoon       = flowing water (tidal wake, crescent smears)
    ///   Incisor           = surgical precision (constellation nodes, standing waves)
    ///   MoonlightsCalling = musical refraction (prismatic scatter, note cascades)
    ///   Resurrection      = HEAVY IMPACT (comet embers, crater rings, supernova bursts, screen shake)
    /// </summary>
    public static class ResurrectionVFX
    {
        // === UNIQUE COLOR ACCENTS — comet/impact palette ===
        public static readonly Color CometCore = new Color(210, 225, 255);
        public static readonly Color CometTrail = new Color(180, 120, 255);
        public static readonly Color ImpactCrater = new Color(100, 80, 200);
        public static readonly Color LunarShine = new Color(120, 190, 255);
        public static readonly Color DeepSpaceViolet = new Color(50, 20, 100);
        public static readonly Color SupernovaWhite = new Color(235, 240, 255);

        /// <summary>
        /// Returns a comet color that shifts from bright blue to deep violet based on ricochet count.
        /// More ricochets = hotter, more white-blue. Like a re-entering meteor heating up.
        /// </summary>
        public static Color GetCometColor(float progress, int ricochetCount)
        {
            float heatShift = MathHelper.Clamp(ricochetCount * 0.06f, 0f, 0.5f);
            Color cold = Color.Lerp(CometTrail, MoonlightVFXLibrary.Violet, progress);
            Color hot = Color.Lerp(LunarShine, CometCore, progress);
            return Color.Lerp(cold, hot, heatShift + progress * 0.3f);
        }

        // === CHAMBER TYPE CONSTANTS ===
        public const int ChamberStandard = 0;
        public const int ChamberCometCore = 1;
        public const int ChamberSupernova = 2;
        public const int ChamberCount = 3;

        /// <summary>
        /// Returns the primary accent color for a given chamber type.
        /// </summary>
        public static Color GetChamberColor(int chamberType)
        {
            return chamberType switch
            {
                ChamberCometCore => CometCore,
                ChamberSupernova => SupernovaWhite,
                _ => CometTrail
            };
        }

        /// <summary>
        /// Returns a secondary (glow) color for a given chamber type.
        /// </summary>
        public static Color GetChamberGlowColor(int chamberType)
        {
            return chamberType switch
            {
                ChamberCometCore => LunarShine,
                ChamberSupernova => ImpactCrater,
                _ => DeepSpaceViolet
            };
        }

        // =====================================================================
        //  MASSIVE MUZZLE FLASH
        // =====================================================================

        /// <summary>
        /// Massive muzzle flash — the gun firing should light up the area.
        /// CometEmberDust blast cone + ResonantPulseDust shockwave + GodRaySystem burst.
        /// </summary>
        public static void MuzzleFlash(Vector2 firePos, Vector2 direction)
        {
            // Flash cascade — 4 layers outward
            CustomParticles.GenericFlare(firePos, Color.White, 1.0f, 16);
            CustomParticles.GenericFlare(firePos, LunarShine, 0.8f, 18);
            CustomParticles.GenericFlare(firePos, CometTrail, 0.6f, 20);
            CustomParticles.GenericFlare(firePos, MoonlightVFXLibrary.DarkPurple, 0.4f, 22);

            // COMET EMBER BLAST CONE — burning fragments erupting from barrel
            for (int i = 0; i < 12; i++)
            {
                Vector2 blastVel = direction.RotatedByRandom(0.4f) * Main.rand.NextFloat(5f, 14f);
                Color emberColor = Color.Lerp(CometCore, LunarShine, Main.rand.NextFloat());
                Dust ember = Dust.NewDustPerfect(firePos,
                    ModContent.DustType<CometEmberDust>(),
                    blastVel, 0, emberColor, 0.35f);
                ember.customData = new CometEmberBehavior
                {
                    VelocityDecay = 0.92f,
                    RotationSpeed = 0.1f + Main.rand.NextFloat(0.05f),
                    BaseScale = 0.3f + Main.rand.NextFloat(0.15f),
                    Lifetime = 25 + Main.rand.Next(10),
                    HasGravity = false
                };
            }

            // RESONANT PULSE SHOCKWAVE — expanding barrel ring
            Dust shockwave = Dust.NewDustPerfect(firePos,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, LunarShine, 0.35f);
            shockwave.customData = new ResonantPulseBehavior
            {
                ExpansionRate = 0.06f,
                ExpansionDecay = 0.93f,
                Lifetime = 18,
                PulseFrequency = 0.2f
            };

            // Perpendicular barrel flash — StarPointDust sparks
            Vector2 perp = new Vector2(-direction.Y, direction.X);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int j = 0; j < 4; j++)
                {
                    Vector2 sparkVel = perp * side * Main.rand.NextFloat(2f, 6f)
                        + direction * Main.rand.NextFloat(1f, 4f);
                    Color sparkColor = Color.Lerp(LunarShine, CometCore, Main.rand.NextFloat());
                    Dust star = Dust.NewDustPerfect(firePos,
                        ModContent.DustType<StarPointDust>(),
                        sparkVel, 0, sparkColor, 0.22f);
                    star.customData = new StarPointBehavior
                    {
                        RotationSpeed = 0.15f,
                        TwinkleFrequency = 0.5f,
                        Lifetime = 20,
                        FadeStartTime = 6
                    };
                }
            }

            // Halo rings
            CustomParticles.HaloRing(firePos, MoonlightVFXLibrary.Violet, 0.55f, 18);
            CustomParticles.HaloRing(firePos, LunarShine, 0.45f, 22);
            CustomParticles.HaloRing(firePos, CometTrail, 0.35f, 26);

            // GodRaySystem — heavy sniper muzzle flash
            GodRaySystem.CreateBurst(firePos, LunarShine, 10, 90f, 25,
                GodRaySystem.GodRayStyle.Explosion, CometTrail);

            // Screen effects
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(firePos, CometTrail, 0.6f, 22);
                MagnumScreenEffects.AddScreenShake(5f);
            }

            // CHROMATIC ABERRATION FLASH
            try
            {
                SLPFlashSystem.SetCAFlashEffect(
                    intensity: 0.15f,
                    lifetime: 12,
                    whiteIntensity: 0.4f,
                    distanceMult: 0.3f,
                    moveIn: true);
            }
            catch { }

            // Music notes
            MoonlightVFXLibrary.SpawnMusicNotes(firePos, 3, 20f, 0.8f, 1.0f, 30);

            Lighting.AddLight(firePos, CometCore.ToVector3() * 1.8f);
        }

        // =====================================================================
        //  COMET TRAIL FRAME (called every frame in projectile AI)
        // =====================================================================

        /// <summary>
        /// Per-frame comet trail — CometEmberDust burning wake, LunarMote orbit,
        /// StarPointDust hot sparks. Intensifies with each ricochet.
        /// </summary>
        public static void CometTrailFrame(Vector2 projCenter, Vector2 velocity, int ricochetCount)
        {
            float bounceIntensity = 1f + ricochetCount * 0.25f;

            // COMET EMBER TRAIL — dense burning fragments behind the projectile
            if (Main.rand.NextBool(2))
            {
                Vector2 dustVel = -velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust ember = Dust.NewDustPerfect(
                    projCenter + Main.rand.NextVector2Circular(5f, 5f),
                    ModContent.DustType<CometEmberDust>(),
                    dustVel, 0, CometCore, 0.25f * bounceIntensity);
                ember.customData = new CometEmberBehavior
                {
                    VelocityDecay = 0.93f,
                    RotationSpeed = 0.08f,
                    BaseScale = 0.25f * bounceIntensity,
                    Lifetime = 22 + ricochetCount * 2,
                    HasGravity = true
                };
            }

            // STAR POINT HOT SPARKS — sharp glinting sparks shed from the comet
            if (Main.rand.NextBool(3))
            {
                Vector2 sparkVel = -velocity * 0.06f + Main.rand.NextVector2Circular(1f, 1f);
                Color sparkColor = Color.Lerp(LunarShine, CometCore, Main.rand.NextFloat(0.5f));
                Dust star = Dust.NewDustPerfect(
                    projCenter + Main.rand.NextVector2Circular(4f, 4f),
                    ModContent.DustType<StarPointDust>(),
                    sparkVel, 0, sparkColor, 0.16f * bounceIntensity);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.12f,
                    TwinkleFrequency = 0.6f,
                    Lifetime = 16,
                    FadeStartTime = 4
                };
            }

            // LUNAR MOTE ORBIT — crescent notes circling the comet
            if (Main.rand.NextBool(6))
            {
                float orbitAngle = Main.GameUpdateCount * 0.12f + Main.rand.NextFloat(MathHelper.TwoPi);
                Color moteColor = Color.Lerp(CometTrail, MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust mote = Dust.NewDustPerfect(projCenter,
                    ModContent.DustType<LunarMote>(),
                    -velocity * 0.03f,
                    0, moteColor, 0.22f * bounceIntensity);
                mote.customData = new LunarMoteBehavior(projCenter, orbitAngle)
                {
                    OrbitRadius = 10f + ricochetCount * 1.5f,
                    OrbitSpeed = 0.14f,
                    Lifetime = 20,
                    FadePower = 0.9f
                };
            }

            // Music notes (every 8 frames)
            if (Main.rand.NextBool(8))
            {
                float orbitAngle = Main.GameUpdateCount * 0.1f;
                for (int i = 0; i < 2; i++)
                {
                    float noteAngle = orbitAngle + MathHelper.Pi * i;
                    Vector2 notePos = projCenter + noteAngle.ToRotationVector2() * 12f;
                    MoonlightVFXLibrary.SpawnMusicNotes(notePos, 1, 3f, 0.7f, 0.85f, 25);
                }
            }

            // Dynamic comet lighting — pulsing between gold and violet
            float lightPulse = 0.85f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.15f;
            Color lightColor = Color.Lerp(CometTrail, LunarShine,
                MathF.Sin(Main.GlobalTimeWrappedHourly * 3f + ricochetCount) * 0.5f + 0.5f);
            Lighting.AddLight(projCenter, lightColor.ToVector3() * (0.7f + ricochetCount * 0.15f) * lightPulse);
        }

        // =====================================================================
        //  RICOCHET VFX (escalating crater impacts)
        // =====================================================================

        /// <summary>
        /// Ricochet VFX — each bounce burns brighter. CometEmberDust spray,
        /// ResonantPulseDust crater rings, StarPointDust shrapnel, LunarMote orbit.
        /// GodRaySystem on ricochets 5+. Screen shake on 7+.
        /// </summary>
        public static void OnRicochetVFX(Vector2 bouncePos, Vector2 outVel, int ricochetCount)
        {
            float intensity = 0.8f + ricochetCount * 0.25f;

            // Central crater flash cascade
            CustomParticles.GenericFlare(bouncePos, Color.White, 0.6f * intensity, 14);
            CustomParticles.GenericFlare(bouncePos, LunarShine, 0.5f * intensity, 16);
            CustomParticles.GenericFlare(bouncePos, CometTrail, 0.4f * intensity, 18);

            // RESONANT PULSE CRATER RINGS — expanding impact shockwaves
            int ringCount = 2 + ricochetCount / 2;
            for (int ring = 0; ring < ringCount; ring++)
            {
                float progress = (float)ring / ringCount;
                Color ringColor = Color.Lerp(ImpactCrater, DeepSpaceViolet, progress);
                Dust pulse = Dust.NewDustPerfect(bouncePos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.22f + ring * 0.05f + ricochetCount * 0.03f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.035f + ring * 0.01f,
                    ExpansionDecay = 0.95f,
                    Lifetime = 16 + ring * 3,
                    PulseFrequency = 0.3f
                };
            }

            // COMET EMBER SPRAY — directional toward outgoing velocity
            int emberCount = 6 + ricochetCount * 2;
            for (int i = 0; i < emberCount; i++)
            {
                Vector2 emberVel = outVel.SafeNormalize(Vector2.Zero).RotatedByRandom(0.8f)
                    * Main.rand.NextFloat(3f, 8f) * intensity;
                Dust ember = Dust.NewDustPerfect(bouncePos,
                    ModContent.DustType<CometEmberDust>(),
                    emberVel, 0, CometCore, 0.28f * intensity);
                ember.customData = new CometEmberBehavior
                {
                    VelocityDecay = 0.93f,
                    RotationSpeed = 0.1f,
                    BaseScale = 0.28f * intensity,
                    Lifetime = 22 + ricochetCount * 2,
                    HasGravity = true
                };
            }

            // STAR POINT SHRAPNEL — sharp sparkles radiating from impact
            int shrapnelCount = 4 + ricochetCount;
            for (int i = 0; i < shrapnelCount; i++)
            {
                float angle = MathHelper.TwoPi * i / shrapnelCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color starColor = Color.Lerp(LunarShine, CometCore, Main.rand.NextFloat(0.5f));
                Dust star = Dust.NewDustPerfect(bouncePos,
                    ModContent.DustType<StarPointDust>(),
                    vel, 0, starColor, 0.2f * intensity);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.15f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 18,
                    FadeStartTime = 5
                };
            }

            // LUNAR MOTE crescent flares at ricochet point
            for (int i = 0; i < 2 + ricochetCount / 3; i++)
            {
                float moteAngle = MathHelper.TwoPi * i / (2 + ricochetCount / 3);
                Color moteColor = Color.Lerp(CometTrail, MoonlightVFXLibrary.IceBlue,
                    (float)i / (2 + ricochetCount / 3));
                Dust mote = Dust.NewDustPerfect(bouncePos,
                    ModContent.DustType<LunarMote>(),
                    moteAngle.ToRotationVector2() * 1.5f,
                    0, moteColor, 0.28f * intensity);
                mote.customData = new LunarMoteBehavior(bouncePos, moteAngle)
                {
                    OrbitRadius = 15f + ricochetCount * 2f,
                    OrbitSpeed = 0.08f,
                    Lifetime = 22,
                    FadePower = 0.92f
                };
            }

            // Gradient halo rings
            CustomParticles.HaloRing(bouncePos, ImpactCrater, 0.3f * intensity, 18);
            CustomParticles.HaloRing(bouncePos, CometTrail, 0.25f * intensity, 22);

            // Music notes — escalating with bounces
            MoonlightVFXLibrary.SpawnMusicNotes(bouncePos, 2 + ricochetCount / 3, 12f, 0.75f, 1.0f, 30);

            // GodRaySystem on ricochets 5+
            if (ricochetCount >= 5)
            {
                GodRaySystem.CreateBurst(bouncePos, CometTrail, 4 + (ricochetCount - 5), 40f + ricochetCount * 5f, 18,
                    GodRaySystem.GodRayStyle.Explosion, LunarShine);
            }

            // Screen shake on ricochets 7+
            if (ricochetCount >= 7)
            {
                MagnumScreenEffects.AddScreenShake(1f + (ricochetCount - 7) * 0.5f);
            }

            Lighting.AddLight(bouncePos, CometCore.ToVector3() * intensity);
        }

        // =====================================================================
        //  ON-HIT EXPLOSION (crater detonation)
        // =====================================================================

        /// <summary>
        /// Crater detonation on NPC hit — CometEmberDust radial lances,
        /// ResonantPulseDust crater rings, StarPointDust shrapnel, moonbeam lances.
        /// Supernova burst on crits with god rays and screen effects.
        /// </summary>
        public static void OnHitExplosion(Vector2 impactPos, int ricochetCount, bool crit)
        {
            float intensity = 1f + ricochetCount * 0.15f;

            // Central impact flash
            CustomParticles.GenericFlare(impactPos, Color.White, 0.7f * intensity, 16);
            CustomParticles.GenericFlare(impactPos, LunarShine, 0.55f * intensity, 18);
            CustomParticles.GenericFlare(impactPos, CometTrail, 0.4f * intensity, 20);

            // Base moonlight impact
            MoonlightVFXLibrary.ProjectileImpact(impactPos, intensity);

            // COMET EMBER RADIAL LANCES — 8-point star of burning ember beams
            int lanceCount = 8;
            for (int i = 0; i < lanceCount; i++)
            {
                float angle = MathHelper.TwoPi * i / lanceCount + Main.rand.NextFloat(-0.1f, 0.1f);
                for (int j = 0; j < 3; j++)
                {
                    Vector2 vel = angle.ToRotationVector2() * (4f + j * 3f);
                    Color lanceColor = GetCometColor((float)j / 3f, ricochetCount);
                    Dust ember = Dust.NewDustPerfect(impactPos,
                        ModContent.DustType<CometEmberDust>(),
                        vel, 0, lanceColor, 0.3f * intensity - j * 0.04f);
                    ember.customData = new CometEmberBehavior
                    {
                        VelocityDecay = 0.94f,
                        RotationSpeed = 0.08f,
                        BaseScale = 0.3f * intensity - j * 0.04f,
                        Lifetime = 22 + ricochetCount * 2,
                        HasGravity = false
                    };
                }
            }

            // RESONANT PULSE CRATER RINGS
            int ringCount = 3 + (crit ? 2 : 0);
            for (int ring = 0; ring < ringCount; ring++)
            {
                float progress = (float)ring / ringCount;
                Color craterColor = Color.Lerp(ImpactCrater, DeepSpaceViolet, progress);
                Dust pulse = Dust.NewDustPerfect(impactPos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, craterColor,
                    0.25f + ring * 0.08f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.04f + ring * 0.012f,
                    ExpansionDecay = 0.94f,
                    Lifetime = 16 + ring * 4,
                    PulseFrequency = 0.25f
                };
            }

            // STAR POINT SHRAPNEL SPRAY
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f);
                Color sparkColor = Color.Lerp(CometCore, LunarShine, Main.rand.NextFloat());
                Dust star = Dust.NewDustPerfect(impactPos,
                    ModContent.DustType<StarPointDust>(),
                    sparkVel, 0, sparkColor, 0.2f * intensity);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.12f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 20,
                    FadeStartTime = 6
                };
            }

            // Gradient halo rings
            for (int ring = 0; ring < 3; ring++)
            {
                float progress = ring / 3f;
                Color haloColor = Color.Lerp(ImpactCrater, CometTrail, progress);
                CustomParticles.HaloRing(impactPos, haloColor, 0.35f + ring * 0.15f, 18 + ring * 5);
            }

            // Music notes
            MoonlightVFXLibrary.SpawnMusicNotes(impactPos, 3, 20f, 0.85f, 1.0f, 30);

            // CRIT: Supernova burst
            if (crit)
            {
                // Extra flare layers
                CustomParticles.GenericFlare(impactPos, SupernovaWhite, 0.85f, 22);
                CustomParticles.GenericFlare(impactPos, MoonlightVFXLibrary.DarkPurple, 0.5f, 24);

                // LunarMote orbit burst
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f;
                    Color moteColor = Color.Lerp(LunarShine, MoonlightVFXLibrary.MoonWhite, (float)i / 4f);
                    Dust mote = Dust.NewDustPerfect(impactPos,
                        ModContent.DustType<LunarMote>(),
                        angle.ToRotationVector2() * 2f,
                        0, moteColor, 0.35f);
                    mote.customData = new LunarMoteBehavior(impactPos, angle)
                    {
                        OrbitRadius = 20f,
                        OrbitSpeed = 0.07f,
                        Lifetime = 25,
                        FadePower = 0.93f
                    };
                }

                GodRaySystem.CreateBurst(impactPos, CometTrail, 6 + ricochetCount / 2, 60f, 22,
                    GodRaySystem.GodRayStyle.Explosion, LunarShine);

                MagnumScreenEffects.AddScreenShake(2f + ricochetCount * 0.3f);
            }

            Lighting.AddLight(impactPos, CometCore.ToVector3() * intensity);
        }

        // =====================================================================
        //  WALL HIT VFX
        // =====================================================================

        /// <summary>
        /// Wall hit VFX — smaller crater impact using custom dusts.
        /// </summary>
        public static void WallHitVFX(Vector2 hitPos)
        {
            MoonlightVFXLibrary.ProjectileImpact(hitPos, 0.6f);

            // CometEmberDust scatter from wall
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Dust ember = Dust.NewDustPerfect(hitPos,
                    ModContent.DustType<CometEmberDust>(),
                    vel, 0, CometCore, 0.22f);
                ember.customData = new CometEmberBehavior(0.22f, 20, true);
            }

            // ResonantPulseDust wall impact ring
            Dust pulse = Dust.NewDustPerfect(hitPos,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, ImpactCrater, 0.2f);
            pulse.customData = new ResonantPulseBehavior(0.03f, 14);

            CustomParticles.HaloRing(hitPos, ImpactCrater, 0.3f, 16);
        }

        // =====================================================================
        //  GRAND FINALE DEATH VFX
        // =====================================================================

        /// <summary>
        /// Grand finale — supernova detonation when projectile expires or exhausts ricochets.
        /// Massive CometEmberDust starburst, expanding ResonantPulseDust shockwaves,
        /// LunarMote orbit, StarPointDust burst, god rays, screen effects.
        /// </summary>
        public static void DeathVFX(Vector2 deathPos, int totalRicochets)
        {
            float intensity = 0.8f + totalRicochets * 0.1f;
            bool isGrandFinale = totalRicochets >= 8;

            // FLARE CASCADE
            CustomParticles.GenericFlare(deathPos, Color.White, 0.7f * intensity, 18);
            CustomParticles.GenericFlare(deathPos, LunarShine, 0.55f * intensity, 20);
            CustomParticles.GenericFlare(deathPos, CometTrail, 0.4f * intensity, 22);

            // COMET EMBER STARBURST
            int emberCount = 8 + totalRicochets;
            for (int i = 0; i < emberCount; i++)
            {
                float angle = MathHelper.TwoPi * i / emberCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Color emberColor = GetCometColor((float)i / emberCount, totalRicochets);
                Dust ember = Dust.NewDustPerfect(deathPos,
                    ModContent.DustType<CometEmberDust>(),
                    vel, 0, emberColor, 0.3f);
                ember.customData = new CometEmberBehavior
                {
                    VelocityDecay = 0.94f,
                    RotationSpeed = 0.1f,
                    BaseScale = 0.3f,
                    Lifetime = 28,
                    HasGravity = true
                };
            }

            // RESONANT PULSE SHOCKWAVES
            int ringCount = 3 + totalRicochets / 3;
            for (int i = 0; i < ringCount; i++)
            {
                Color ringColor = Color.Lerp(ImpactCrater, CometTrail, (float)i / ringCount);
                Dust pulse = Dust.NewDustPerfect(deathPos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.22f + i * 0.06f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.035f + i * 0.01f,
                    ExpansionDecay = 0.94f,
                    Lifetime = 16 + i * 4,
                    PulseFrequency = 0.25f
                };
            }

            // STAR POINT BURST
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color starColor = Color.Lerp(LunarShine, CometCore, Main.rand.NextFloat());
                Dust star = Dust.NewDustPerfect(deathPos,
                    ModContent.DustType<StarPointDust>(),
                    vel, 0, starColor, 0.22f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.12f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 24,
                    FadeStartTime = 7
                };
            }

            // Halo ring cascade
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = Color.Lerp(CometTrail, LunarShine, i / 3f);
                CustomParticles.HaloRing(deathPos, ringColor, 0.25f + i * 0.1f, 14 + i * 4);
            }

            // Music note cascade
            MoonlightVFXLibrary.SpawnMusicNotes(deathPos, 4, 25f, 0.8f, 1.0f, 35);

            // GRAND FINALE EXTRAS (8+ ricochets achieved)
            if (isGrandFinale)
            {
                // Extra supernova flares
                CustomParticles.GenericFlare(deathPos, SupernovaWhite, 1.0f * intensity, 24);
                CustomParticles.GenericFlare(deathPos, MoonlightVFXLibrary.DarkPurple, 0.7f * intensity, 26);

                // LunarMote supernova orbit — 6 crescents spiraling outward
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Color moteColor = Color.Lerp(LunarShine, MoonlightVFXLibrary.MoonWhite, (float)i / 6f);
                    Dust mote = Dust.NewDustPerfect(deathPos + angle.ToRotationVector2() * 10f,
                        ModContent.DustType<LunarMote>(),
                        angle.ToRotationVector2() * 2f,
                        0, moteColor, 0.45f);
                    mote.customData = new LunarMoteBehavior(deathPos, angle)
                    {
                        OrbitRadius = 25f + i * 5f,
                        OrbitSpeed = 0.06f,
                        Lifetime = 35,
                        FadePower = 0.93f
                    };
                }

                // Moonlight lightning fractals
                for (int i = 0; i < 4; i++)
                {
                    float lightningAngle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 lightningEnd = deathPos + lightningAngle.ToRotationVector2() * 70f;
                    MagnumVFX.DrawMoonlightLightning(deathPos, lightningEnd, 5, 18f, 2, 0.35f);
                }

                // GOD RAY BURST — supernova finale
                GodRaySystem.CreateBurst(deathPos, LunarShine, 8, 80f, 28,
                    GodRaySystem.GodRayStyle.Explosion, CometTrail);

                // SCREEN EFFECTS
                if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
                {
                    ScreenDistortionManager.TriggerRipple(deathPos, CometTrail, 0.5f, 22);
                    MagnumScreenEffects.AddScreenShake(4f);
                }

                // CHROMATIC ABERRATION
                try
                {
                    SLPFlashSystem.SetCAFlashEffect(
                        intensity: 0.18f,
                        lifetime: 14,
                        whiteIntensity: 0.45f,
                        distanceMult: 0.35f,
                        moveIn: true);
                }
                catch { }

                // Supernova arc burst
                CustomParticles.SwordArcBurst(deathPos, LunarShine, 8, 0.5f);
            }

            Lighting.AddLight(deathPos, CometCore.ToVector3() * intensity);
        }

        // =====================================================================
        //  BEAM BODY BLOOM
        // =====================================================================

        /// <summary>
        /// Projectile body bloom — comet-core 5-layer bloom stack using {A=0}.
        /// Color shifts hotter with more ricochets.
        /// </summary>
        public static void DrawProjectileBloom(SpriteBatch sb, Vector2 projWorldPos, float velocityMagnitude, int ricochetCount)
        {
            if (sb == null) return;

            Vector2 drawPos = projWorldPos - Main.screenPosition;
            float speed = MathHelper.Clamp(velocityMagnitude / 20f, 0.5f, 1.5f);
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.08f;
            float bounceScale = 1f + ricochetCount * 0.12f;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            // Layer 1: Outer comet tail glow (color heats up with ricochets)
            Color outerColor = GetCometColor(Main.GlobalTimeWrappedHourly % 1f, ricochetCount);
            sb.Draw(bloomTex, drawPos, null,
                (outerColor with { A = 0 }) * 0.2f,
                0f, origin, 0.75f * speed * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 2: Deep space violet mid
            sb.Draw(bloomTex, drawPos, null,
                (DeepSpaceViolet with { A = 0 }) * 0.25f,
                0f, origin, 0.55f * speed * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 3: Comet trail violet
            sb.Draw(bloomTex, drawPos, null,
                (CometTrail with { A = 0 }) * 0.40f,
                0f, origin, 0.38f * speed * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 4: Moonrise gold inner
            sb.Draw(bloomTex, drawPos, null,
                (LunarShine with { A = 0 }) * 0.60f,
                0f, origin, 0.22f * speed * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 5: White-hot comet core
            sb.Draw(bloomTex, drawPos, null,
                (Color.White with { A = 0 }) * 0.80f,
                0f, origin, 0.10f * speed * bounceScale * pulse, SpriteEffects.None, 0f);
        }

        // =====================================================================
        //  RELOAD READY FLASH
        // =====================================================================

        /// <summary>
        /// Reload ready flash — CometEmberDust burst + ResonantPulseDust ring
        /// when sniper is charged and ready to fire.
        /// </summary>
        public static void ReadyFlash(Vector2 gunPos)
        {
            // Central flash
            CustomParticles.GenericFlare(gunPos, Color.White, 0.7f, 20);
            CustomParticles.GenericFlare(gunPos, MoonlightVFXLibrary.IceBlue, 0.6f, 18);
            CustomParticles.GenericFlare(gunPos, CometTrail, 0.45f, 16);

            // CometEmberDust burst — radial ember explosion
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Dust ember = Dust.NewDustPerfect(gunPos,
                    ModContent.DustType<CometEmberDust>(),
                    vel, 0, LunarShine, 0.22f);
                ember.customData = new CometEmberBehavior(0.22f, 20, false);
            }

            // ResonantPulseDust ready ring
            Dust pulse = Dust.NewDustPerfect(gunPos,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, MoonlightVFXLibrary.IceBlue, 0.25f);
            pulse.customData = new ResonantPulseBehavior(0.04f, 16);

            // StarPointDust fractal sparkles
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = angle.ToRotationVector2() * 20f;
                Color starColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple,
                    MoonlightVFXLibrary.MoonWhite, (float)i / 6f);
                Dust star = Dust.NewDustPerfect(gunPos + flareOffset,
                    ModContent.DustType<StarPointDust>(),
                    Vector2.Zero, 0, starColor, 0.18f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.1f,
                    TwinkleFrequency = 0.4f,
                    Lifetime = 22,
                    FadeStartTime = 6
                };
            }

            // Halo ring
            CustomParticles.HaloRing(gunPos, MoonlightVFXLibrary.IceBlue, 0.4f, 18);

            // Music notes
            MoonlightVFXLibrary.SpawnMusicNotes(gunPos, 4, 25f, 0.8f, 1.0f, 30);

            Lighting.AddLight(gunPos, MoonlightVFXLibrary.MoonWhite.ToVector3() * 1.2f);
        }

        /// <summary>
        /// Dynamic comet lighting — pulsing between gold-white and violet.
        /// </summary>
        public static void AddCometLight(Vector2 worldPos, float intensity = 0.7f, int ricochetCount = 0)
        {
            float pulse = 0.85f + MathF.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.15f;
            Color lightColor = Color.Lerp(CometTrail, LunarShine,
                MathF.Sin(Main.GlobalTimeWrappedHourly * 3f + ricochetCount) * 0.5f + 0.5f);
            Lighting.AddLight(worldPos, lightColor.ToVector3() * intensity * pulse);
        }

        // =====================================================================
        //  SHARED SHADER RENDERING HELPERS
        // =====================================================================

        /// <summary>
        /// Shared CometTrail.fx shader glow overlay renderer.
        /// Draws a burning comet ember effect at the specified world position.
        /// Two passes: main body (1.2x scale) + soft glow bloom (1.8x scale).
        /// Used by all Resurrection projectiles for consistent shader visualization.
        ///
        /// Call from PreDraw (within the SpriteBatch context).
        /// </summary>
        public static void DrawCometShaderGlow(SpriteBatch sb, Vector2 worldPos,
            float rotation, float cometPhase, float baseScale = 0.15f)
        {
            if (!MoonlightSonataShaderManager.HasCometTrail) return;

            var glowTex = MoonlightSonataTextures.BloomOrb?.Value
                       ?? MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = glowTex.Size() * 0.5f;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f + cometPhase * 10f) * 0.08f;
            float glowScale = baseScale * pulse;

            try
            {
                MoonlightSonataShaderManager.BeginShaderBatch(sb);

                // Pass 1: Main burning ember body
                MoonlightSonataShaderManager.ApplyResurrectionCometTrail(
                    Main.GlobalTimeWrappedHourly, cometPhase, glowPass: false);

                sb.Draw(glowTex, drawPos, null,
                    Color.White, rotation, origin,
                    glowScale * 1.2f, SpriteEffects.None, 0f);

                // Pass 2: Soft glow bloom
                MoonlightSonataShaderManager.ApplyResurrectionCometTrail(
                    Main.GlobalTimeWrappedHourly, cometPhase, glowPass: true);

                sb.Draw(glowTex, drawPos, null,
                    Color.White * 0.7f, rotation, origin,
                    glowScale * 1.8f, SpriteEffects.None, 0f);

                MoonlightSonataShaderManager.RestoreDefaultBatch(sb);
            }
            catch
            {
                // Fallback: plain additive bloom if shader fails
                try { MoonlightSonataShaderManager.RestoreDefaultBatch(sb); } catch { }
                sb.Draw(glowTex, drawPos, null,
                    MoonlightSonataPalette.Additive(CometTrail, 0.3f),
                    rotation, origin, glowScale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws SupernovaBlast.fx shader overlay at a detonation point.
        /// Creates a GPU-rendered radial crater explosion that expands over its lifetime.
        /// Two passes: main blast body + ring-only expanding shockwave.
        ///
        /// explosionAge: 0 = just detonated, 1 = fully expanded/faded.
        /// Call from PreDraw or as a one-shot overlay during death/crit VFX rendering.
        /// </summary>
        public static void DrawSupernovaBlastOverlay(SpriteBatch sb, Vector2 worldPos,
            float explosionAge, float scale = 0.5f)
        {
            if (!MoonlightSonataShaderManager.HasSupernovaBlast) return;

            var glowTex = MoonlightSonataTextures.BloomOrb?.Value
                       ?? MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = glowTex.Size() * 0.5f;
            float expandScale = scale * (1f + explosionAge * 2f);
            float fadeAlpha = MathHelper.Clamp(1f - explosionAge * 0.6f, 0.1f, 1f);

            try
            {
                MoonlightSonataShaderManager.BeginShaderBatch(sb);

                // Pass 1: Main blast body
                MoonlightSonataShaderManager.ApplyResurrectionSupernovaBlast(
                    Main.GlobalTimeWrappedHourly, explosionAge, ringOnly: false);

                sb.Draw(glowTex, drawPos, null,
                    Color.White * fadeAlpha, 0f, origin,
                    expandScale * 1.5f, SpriteEffects.None, 0f);

                // Pass 2: Expanding ring shockwave
                MoonlightSonataShaderManager.ApplyResurrectionSupernovaBlast(
                    Main.GlobalTimeWrappedHourly, explosionAge, ringOnly: true);

                sb.Draw(glowTex, drawPos, null,
                    Color.White * fadeAlpha * 0.6f, 0f, origin,
                    expandScale * 2.2f, SpriteEffects.None, 0f);

                MoonlightSonataShaderManager.RestoreDefaultBatch(sb);
            }
            catch
            {
                try { MoonlightSonataShaderManager.RestoreDefaultBatch(sb); } catch { }
                // Fallback: additive flare
                sb.Draw(glowTex, drawPos, null,
                    MoonlightSonataPalette.Additive(ImpactCrater, 0.3f * fadeAlpha),
                    0f, origin, expandScale, SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        //  CHAMBER MECHANIC VFX
        // =====================================================================

        /// <summary>
        /// Per-frame Chamber charge indicator VFX at the gun barrel.
        /// Spawns orbiting dust in the active chamber's color scheme,
        /// converging toward the barrel as charge completes.
        /// </summary>
        public static void ChamberChargeFrame(Vector2 gunPos, int chamberType, float chargeProgress)
        {
            Color primaryColor = GetChamberColor(chamberType);
            Color glowColor = GetChamberGlowColor(chamberType);
            float converge = 1f - chargeProgress; // 1 = far out, 0 = converged at barrel

            // Orbiting chamber indicator dust — converges inward with progress
            if (Main.GameUpdateCount % 5 == 0)
            {
                float orbitPhase = Main.GameUpdateCount * (0.12f + chargeProgress * 0.18f);
                for (int i = 0; i < 3; i++)
                {
                    float angle = orbitPhase + MathHelper.TwoPi * i / 3f;
                    float radius = 8f + converge * 18f;
                    Vector2 orbitPos = gunPos + angle.ToRotationVector2() * radius;
                    Vector2 vel = (gunPos - orbitPos).SafeNormalize(Vector2.Zero) * (1f + chargeProgress * 2.5f);

                    Color dustColor = Color.Lerp(glowColor, primaryColor, chargeProgress);
                    Dust ember = Dust.NewDustPerfect(orbitPos,
                        ModContent.DustType<CometEmberDust>(),
                        vel, 0, dustColor, 0.18f + chargeProgress * 0.12f);
                    ember.customData = new CometEmberBehavior
                    {
                        VelocityDecay = 0.9f,
                        RotationSpeed = 0.08f,
                        BaseScale = 0.18f + chargeProgress * 0.12f,
                        Lifetime = 16,
                        HasGravity = false
                    };
                }
            }

            // StarPointDust convergence sparkles
            if (Main.rand.NextBool(5))
            {
                Vector2 sparkStart = gunPos + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 sparkVel = (gunPos - sparkStart).SafeNormalize(Vector2.Zero) * (1.5f + chargeProgress * 3f);
                Dust star = Dust.NewDustPerfect(sparkStart,
                    ModContent.DustType<StarPointDust>(),
                    sparkVel, 0, primaryColor, 0.14f + chargeProgress * 0.06f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.1f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 18,
                    FadeStartTime = 6
                };
            }

            // Chamber-specific per-frame lighting
            float lightPulse = 0.3f + chargeProgress * 0.3f +
                MathF.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.1f;
            Lighting.AddLight(gunPos, primaryColor.ToVector3() * lightPulse);
        }

        /// <summary>
        /// One-shot burst VFX when switching between chamber ammo types.
        /// Radial burst in outgoing chamber's color + flash in incoming chamber's color.
        /// </summary>
        public static void ChamberSwitchVFX(Vector2 gunPos, int fromChamber, int toChamber)
        {
            Color fromColor = GetChamberColor(fromChamber);
            Color toColor = GetChamberColor(toChamber);
            Color toGlow = GetChamberGlowColor(toChamber);

            // Outgoing chamber — dispersing ember burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Dust ember = Dust.NewDustPerfect(gunPos,
                    ModContent.DustType<CometEmberDust>(),
                    vel, 0, fromColor, 0.2f);
                ember.customData = new CometEmberBehavior(0.2f, 18, false);
            }

            // Incoming chamber — converging flash
            CustomParticles.GenericFlare(gunPos, Color.White, 0.5f, 12);
            CustomParticles.GenericFlare(gunPos, toColor, 0.4f, 14);

            // ResonantPulseDust transition ring
            Dust pulse = Dust.NewDustPerfect(gunPos,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, toColor, 0.22f);
            pulse.customData = new ResonantPulseBehavior(0.04f, 14);

            // StarPointDust sparkle ring in new chamber color
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 sparkOffset = angle.ToRotationVector2() * 12f;
                Dust star = Dust.NewDustPerfect(gunPos + sparkOffset,
                    ModContent.DustType<StarPointDust>(),
                    Vector2.Zero, 0, toGlow, 0.16f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.1f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 16,
                    FadeStartTime = 4
                };
            }

            // Halo ring in incoming chamber color
            CustomParticles.HaloRing(gunPos, toColor, 0.3f, 16);

            Lighting.AddLight(gunPos, toColor.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Chamber-specific muzzle flash variant — accents the standard MuzzleFlash
        /// with the active chamber's color palette. Call after MuzzleFlash().
        /// </summary>
        public static void ChamberMuzzleAccent(Vector2 firePos, Vector2 direction, int chamberType)
        {
            if (chamberType == ChamberStandard) return; // Standard uses default MuzzleFlash

            Color chamberColor = GetChamberColor(chamberType);
            Color chamberGlow = GetChamberGlowColor(chamberType);

            // Extra flare in chamber color
            CustomParticles.GenericFlare(firePos, chamberColor, 0.7f, 14);
            CustomParticles.GenericFlare(firePos, chamberGlow, 0.5f, 16);

            // Chamber-colored ember blast cone
            for (int i = 0; i < 6; i++)
            {
                Vector2 blastVel = direction.RotatedByRandom(0.5f) * Main.rand.NextFloat(4f, 10f);
                Dust ember = Dust.NewDustPerfect(firePos,
                    ModContent.DustType<CometEmberDust>(),
                    blastVel, 0, chamberColor, 0.3f);
                ember.customData = new CometEmberBehavior
                {
                    VelocityDecay = 0.92f,
                    RotationSpeed = 0.1f,
                    BaseScale = 0.3f,
                    Lifetime = 20,
                    HasGravity = false
                };
            }

            // ResonantPulseDust chamber ring
            Dust pulse = Dust.NewDustPerfect(firePos,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, chamberColor, 0.3f);
            pulse.customData = new ResonantPulseBehavior
            {
                ExpansionRate = 0.05f,
                ExpansionDecay = 0.93f,
                Lifetime = 16,
                PulseFrequency = 0.2f
            };

            // Supernova chamber gets extra god ray burst
            if (chamberType == ChamberSupernova)
            {
                GodRaySystem.CreateBurst(firePos, SupernovaWhite, 6, 60f, 20,
                    GodRaySystem.GodRayStyle.Explosion, ImpactCrater);
                MagnumScreenEffects.AddScreenShake(3f);
            }

            Lighting.AddLight(firePos, chamberColor.ToVector3() * 1.2f);
        }
    }
}
