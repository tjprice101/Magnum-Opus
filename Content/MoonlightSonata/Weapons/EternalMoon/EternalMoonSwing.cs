using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Projectiles;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.SandboxLastPrism.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon
{
    /// <summary>
    /// Swing projectile for EternalMoon — "The Eternal Tide".
    /// 5-phase Tidal Lunar Cycle combo (New Moon → Waxing → Half → Waning → Full Moon).
    ///
    /// Attack pattern escalation across the full lunar cycle:
    ///   Phase 0 "New Moon" (Whisper):       2 waves in narrow cone — surgical, minimal dust
    ///   Phase 1 "Waxing Crescent" (Rising): 3 waves + 2 tidal wash — trail shader activates
    ///   Phase 2 "Half Moon" (Equilibrium):  4 waves + 2 tidal wash + ghost reflection swing
    ///   Phase 3 "Waning Gibbous" (Surge):   4 waves + 3 tidal wash + 2 beams — screen distortion
    ///   Phase 4 "Full Moon" (Crescendo):    6 waves radial + 4 wash + 3 beams + tidal detonation
    ///
    /// VFX pipeline (sealed in MeleeSwingBase):
    ///   Trail (Cosmic) → Smear → Blade → Glow → LensFlare → MotionBlur → CustomVFX
    ///
    /// Custom VFX layer adds:
    ///   - TidalTrail.fx shader-driven flowing water trail (phases 1+)
    ///   - CrescentBloom.fx shader-driven crescent moon overlay at blade tip
    ///   - LunarPhaseAura.fx radial aura around player (phases 3+)
    ///   - Tidal wake system (flowing afterwash from blade positions)
    ///   - TidalMoonDust/LunarMote/StarPointDust flowing particles
    ///   - Phase transition eclipse flashes
    ///   - God rays + screen distortion + chromatic aberration on Full Moon crescendo
    /// </summary>
    public sealed class EternalMoonSwing : MeleeSwingBase
    {
        #region Fields

        private int _lastComboStep = -1;
        private bool _hasTriggeredSecondary;
        private bool _hasTriggeredTertiary;

        #endregion

        #region Palette

        // Use the canonical palette from MoonlightSonataPalette
        private static readonly Color[] EternalMoonPalette = MoonlightSonataPalette.EternalMoonBlade;

        #endregion

        #region Combo Phases — 5-Phase Tidal Lunar Cycle

        // Phase 0: "New Moon" (Whisper) — subtle horizontal sweep, thin crescent arc
        private static readonly ComboPhase Phase0_NewMoon = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -0.80f, 0.15f, 2),
                new CurveSegment(EasingType.PolyIn, 0.18f, -0.65f, 1.35f, 3),
                new CurveSegment(EasingType.PolyOut, 0.80f, 0.70f, 0.08f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.3f,
            duration: 24,
            bladeLength: 150f,
            flip: false,
            squish: 0.93f,
            damageMult: 0.85f
        );

        // Phase 1: "Waxing Crescent" (Rising) — wider diagonal slash, growing momentum
        private static readonly ComboPhase Phase1_WaxingCrescent = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, 0.75f, -0.15f, 2),
                new CurveSegment(EasingType.PolyIn, 0.15f, 0.60f, -1.50f, 3),
                new CurveSegment(EasingType.PolyOut, 0.75f, -0.90f, -0.08f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.5f,
            duration: 26,
            bladeLength: 155f,
            flip: true,
            squish: 0.90f,
            damageMult: 0.95f
        );

        // Phase 2: "Half Moon" (Equilibrium) — full horizontal sweep with reversed direction
        private static readonly ComboPhase Phase2_HalfMoon = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -0.95f, 0.20f, 2),
                new CurveSegment(EasingType.PolyIn, 0.20f, -0.75f, 1.70f, 3),
                new CurveSegment(EasingType.PolyOut, 0.82f, 0.95f, 0.10f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.7f,
            duration: 28,
            bladeLength: 160f,
            flip: false,
            squish: 0.88f,
            damageMult: 1.05f
        );

        // Phase 3: "Waning Gibbous" (Surge) — heavy overhead slam with wind-up pause
        private static readonly ComboPhase Phase3_WaningGibbous = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, 1.10f, -0.10f, 2),   // Wind-up hold
                new CurveSegment(EasingType.SineIn, 0.10f, 1.00f, -0.06f, 2), // Pause at apex
                new CurveSegment(EasingType.PolyIn, 0.18f, 0.94f, -2.20f, 4), // Devastating slam
                new CurveSegment(EasingType.PolyOut, 0.82f, -1.26f, -0.08f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 2.0f,
            duration: 34,
            bladeLength: 165f,
            flip: true,
            squish: 0.84f,
            damageMult: 1.30f
        );

        // Phase 4: "Full Moon" (Crescendo Tide) — massive 360-degree spinning slash
        private static readonly ComboPhase Phase4_FullMoon = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1.25f, 0.15f, 2),   // Gather
                new CurveSegment(EasingType.SineIn, 0.12f, -1.10f, 0.10f, 2), // Tension
                new CurveSegment(EasingType.PolyIn, 0.22f, -1.00f, 2.80f, 4), // Explosive spin
                new CurveSegment(EasingType.PolyOut, 0.85f, 1.80f, 0.05f, 2)  // Follow-through
            },
            maxAngle: MathHelper.PiOver2 * 2.8f,  // Nearly full rotation
            duration: 42,
            bladeLength: 175f,
            flip: false,
            squish: 0.78f,
            damageMult: 1.65f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new ComboPhase[]
        {
            Phase0_NewMoon,
            Phase1_WaxingCrescent,
            Phase2_HalfMoon,
            Phase3_WaningGibbous,
            Phase4_FullMoon
        };

        protected override Color[] GetPalette() => EternalMoonPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Cosmic;

        protected override string GetSmearTexturePath(int comboStep) => comboStep switch
        {
            0 => "MagnumOpus/Assets/Particles/SwordArc2",
            1 => "MagnumOpus/Assets/Particles/SwordArc3",
            2 => "MagnumOpus/Assets/Particles/SwordArc3",
            3 => "MagnumOpus/Assets/Particles/FlamingArcSwordSlash",
            4 => "MagnumOpus/Assets/Particles/FlamingArcSwordSlash",
            _ => "MagnumOpus/Assets/Particles/SwordArc2"
        };

        #endregion

        #region Virtual Overrides

        protected override SoundStyle GetSwingSound() => ComboStep switch
        {
            0 => SoundID.Item71 with { Pitch = -0.3f, Volume = 0.75f },
            1 => SoundID.Item71 with { Pitch = -0.15f, Volume = 0.80f },
            2 => SoundID.Item71 with { Pitch = 0f, Volume = 0.85f },
            3 => SoundID.Item71 with { Pitch = 0.15f, Volume = 0.90f },
            4 => SoundID.Item122 with { Pitch = -0.2f, Volume = 0.95f },
            _ => SoundID.Item71 with { Pitch = -0.2f, Volume = 0.85f }
        };

        protected override int GetInitialDustType() => DustID.PurpleTorch;

        protected override int GetSecondaryDustType() => DustID.Enchanted_Pink;

        protected override Texture2D GetBladeTexture()
            => ModContent.Request<Texture2D>("MagnumOpus/Content/MoonlightSonata/Weapons/EternalMoon/EternalMoon").Value;

        protected override Vector3 GetLightColor()
        {
            float phase = GetMoonPhase(ComboStep);
            float intensity = 0.5f + phase * 0.5f;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4f + ComboStep * 0.8f) * 0.12f;
            Color c = MoonlightSonataPalette.PaletteLerp(EternalMoonPalette, Progression * 0.7f + phase * 0.3f);
            return c.ToVector3() * intensity * pulse;
        }

        /// <summary>Maps combo step to lunar phase intensity (0-1).</summary>
        private static float GetMoonPhase(int comboStep) => comboStep switch
        {
            0 => 0.15f,  // New Moon — barely visible
            1 => 0.35f,  // Waxing Crescent
            2 => 0.55f,  // Half Moon
            3 => 0.80f,  // Waning Gibbous
            4 => 1.00f,  // Full Moon
            _ => 0.5f
        };

        protected override void InitializeSwing()
        {
            base.InitializeSwing();

            EternalMoonVFX.ResetWakeTracking();
            _hasTriggeredSecondary = false;
            _hasTriggeredTertiary = false;

            // Phase transition eclipse flash when combo advances
            if (_lastComboStep >= 0 && ComboStep != _lastComboStep)
            {
                EternalMoonVFX.PhaseTransitionEclipse(Owner.Center, ComboStep);

                // Transition sound — pitch rises with lunar cycle
                SoundEngine.PlaySound(SoundID.Item29 with
                {
                    Pitch = -0.2f + ComboStep * 0.12f,
                    Volume = 0.35f + ComboStep * 0.05f
                }, Owner.Center);

                // Phase 3+: deeper resonance sound layered on top
                if (ComboStep >= 3)
                {
                    SoundEngine.PlaySound(SoundID.Item105 with
                    {
                        Pitch = -0.3f + ComboStep * 0.1f,
                        Volume = 0.30f
                    }, Owner.Center);
                }
            }
            _lastComboStep = ComboStep;
        }

        protected override void DoBehavior_Swinging()
        {
            base.DoBehavior_Swinging();

            // Record tidal wake positions during active swing
            if (Progression > 0.1f && Progression < 0.9f)
            {
                Vector2 midBlade = Vector2.Lerp(Owner.MountedCenter, GetBladeTipPosition(), 0.6f);
                EternalMoonVFX.RecordWakePosition(midBlade, SwordDirection.ToRotation());
            }
        }

        #endregion

        #region Combo Specials — 5-Phase Tidal Lunar Cycle

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // === Phase 0 "New Moon" (Whisper): 2 waves in narrow cone ===
            if (ComboStep == 0 && Progression >= 0.65f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    float baseAngle = SwordDirection.ToRotation();

                    // 2 EternalMoonWave — tight spread, low power
                    for (int i = -1; i <= 1; i += 2)
                    {
                        float spreadAngle = baseAngle + MathHelper.ToRadians(8f * i);
                        Vector2 vel = spreadAngle.ToRotationVector2() * 13f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<EternalMoonWave>(),
                            (int)(Projectile.damage * 0.45f),
                            Projectile.knockBack * 0.4f, Projectile.owner);
                    }
                }

                // Subtle VFX — thin crescent arc
                CustomParticles.GenericFlare(tipPos, MoonlightSonataPalette.IceBlue, 0.35f, 12);
            }

            // === Phase 1 "Waxing Crescent" (Rising): 3 waves + 2 tidal wash ===
            if (ComboStep == 1 && Progression >= 0.60f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    float baseAngle = SwordDirection.ToRotation();

                    // 3 EternalMoonWave in fan
                    for (int i = -1; i <= 1; i++)
                    {
                        float spreadAngle = baseAngle + MathHelper.ToRadians(14f * i);
                        Vector2 vel = spreadAngle.ToRotationVector2() * 14f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<EternalMoonWave>(),
                            (int)(Projectile.damage * 0.50f),
                            Projectile.knockBack * 0.5f, Projectile.owner);
                    }

                    // 2 EternalMoonTidalWash — curving left and right
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 washVel = SwordDirection * 11f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, washVel,
                            ModContent.ProjectileType<EternalMoonTidalWash>(),
                            (int)(Projectile.damage * 0.40f),
                            2f, Projectile.owner, ai0: i);
                    }
                }

                // VFX — crescent bloom + halo ring
                CustomParticles.GenericFlare(tipPos, MoonlightSonataPalette.IceBlue, 0.5f, 14);
                CustomParticles.HaloRing(tipPos, EternalMoonVFX.TidalFoam, 0.3f, 12);

                // Tidal dust spray
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f;
                    Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 3.5f);
                    Color dustColor = EternalMoonVFX.GetLunarPhaseColor((float)i / 4f, 1);
                    Dust tidal = Dust.NewDustPerfect(tipPos,
                        ModContent.DustType<TidalMoonDust>(),
                        dustVel, 0, dustColor, 0.28f);
                    tidal.customData = new TidalMoonBehavior(2f, 18);
                }
            }

            // === Phase 2 "Half Moon" (Equilibrium): 4 waves + 2 tidal wash + ghost reflection ===
            if (ComboStep == 2 && Progression >= 0.58f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    float baseAngle = SwordDirection.ToRotation();

                    // 4 EternalMoonWave — wider fan
                    for (int i = -1; i <= 2; i++)
                    {
                        float spreadAngle = baseAngle + MathHelper.ToRadians(12f * (i - 0.5f));
                        Vector2 vel = spreadAngle.ToRotationVector2() * 15f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<EternalMoonWave>(),
                            (int)(Projectile.damage * 0.55f),
                            Projectile.knockBack * 0.5f, Projectile.owner);
                    }

                    // 2 EternalMoonTidalWash — opposing curves
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 washVel = SwordDirection.RotatedBy(MathHelper.ToRadians(8f * i)) * 12f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, washVel,
                            ModContent.ProjectileType<EternalMoonTidalWash>(),
                            (int)(Projectile.damage * 0.45f),
                            2.5f, Projectile.owner, ai0: i);
                    }

                    // NEW: Ghost Reflection — mirrored translucent swing
                    Vector2 reflectVel = SwordDirection.RotatedBy(MathHelper.Pi * 0.15f * -Owner.direction) * 8f;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos, reflectVel,
                        ModContent.ProjectileType<EternalMoonReflection>(),
                        (int)(Projectile.damage * 0.30f),
                        Projectile.knockBack * 0.2f, Projectile.owner,
                        ai0: -Owner.direction);
                }

                // VFX — dual flare + equilibrium ring
                CustomParticles.GenericFlare(tipPos, Color.White, 0.55f, 15);
                CustomParticles.GenericFlare(tipPos, MoonlightSonataPalette.Violet, 0.4f, 13);
                CustomParticles.HaloRing(tipPos, EternalMoonVFX.DeepTide, 0.35f, 14);

                // Tidal dust burst
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                    Color dustColor = EternalMoonVFX.GetLunarPhaseColor((float)i / 6f, 2);
                    Dust tidal = Dust.NewDustPerfect(tipPos,
                        ModContent.DustType<TidalMoonDust>(),
                        dustVel, 0, dustColor, 0.32f);
                    tidal.customData = new TidalMoonBehavior(2.5f, 22);
                }
            }

            // Phase 2 secondary: additional tidal wash at later progression
            if (ComboStep == 2 && !_hasTriggeredSecondary && Progression >= 0.82f)
            {
                _hasTriggeredSecondary = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos,
                        SwordDirection * 10f,
                        ModContent.ProjectileType<EternalMoonTidalWash>(),
                        (int)(Projectile.damage * 0.35f),
                        2f, Projectile.owner, ai0: Owner.direction);
                }

                Dust pulse = Dust.NewDustPerfect(tipPos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, EternalMoonVFX.TidalFoam, 0.25f);
                pulse.customData = new ResonantPulseBehavior(0.04f, 16);
            }

            // === Phase 3 "Waning Gibbous" (Surge): 4 waves + 3 wash + 2 beams ===
            if (ComboStep == 3 && Progression >= 0.52f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    float baseAngle = SwordDirection.ToRotation();

                    // 4 EternalMoonWave in wide fan
                    for (int i = -1; i <= 2; i++)
                    {
                        float spreadAngle = baseAngle + MathHelper.ToRadians(15f * (i - 0.5f));
                        Vector2 vel = spreadAngle.ToRotationVector2() * 16f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<EternalMoonWave>(),
                            (int)(Projectile.damage * 0.55f),
                            Projectile.knockBack * 0.6f, Projectile.owner);
                    }

                    // 3 EternalMoonTidalWash — fan of curves
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 washVel = SwordDirection.RotatedBy(MathHelper.ToRadians(10f * i)) * 12f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, washVel,
                            ModContent.ProjectileType<EternalMoonTidalWash>(),
                            (int)(Projectile.damage * 0.50f),
                            3f, Projectile.owner,
                            ai0: i == 0 ? Owner.direction : i);
                    }

                    // 2 EternalMoonBeam — homing crystals
                    for (int i = -1; i <= 1; i += 2)
                    {
                        float beamAngle = baseAngle + MathHelper.ToRadians(18f * i);
                        Vector2 beamVel = beamAngle.ToRotationVector2() * 10f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, beamVel,
                            ModContent.ProjectileType<EternalMoonBeam>(),
                            (int)(Projectile.damage * 1.2f),
                            Projectile.knockBack * 0.3f, Projectile.owner);
                    }
                }

                // Heavy impact VFX
                CustomParticles.GenericFlare(tipPos, Color.White, 0.7f, 18);
                CustomParticles.GenericFlare(tipPos, MoonlightSonataPalette.Violet, 0.5f, 15);
                CustomParticles.GenericFlare(tipPos, MoonlightSonataPalette.IceBlue, 0.4f, 13);
                CustomParticles.HaloRing(tipPos, EternalMoonVFX.CrescentGlow, 0.4f, 16);

                // Screen distortion on release
                if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
                {
                    ScreenDistortionManager.TriggerRipple(tipPos, MoonlightSonataPalette.Violet, 0.4f, 18);
                    MagnumScreenEffects.AddScreenShake(3f);
                }

                // Tidal dust burst
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 5f);
                    Color dustColor = EternalMoonVFX.GetLunarPhaseColor((float)i / 8f, 3);
                    Dust tidal = Dust.NewDustPerfect(tipPos,
                        ModContent.DustType<TidalMoonDust>(),
                        dustVel, 0, dustColor, 0.38f);
                    tidal.customData = new TidalMoonBehavior(3.5f, 25);
                }

                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.55f, Pitch = 0f }, tipPos);
            }

            // Phase 3 secondary: additional beams at later progression
            if (ComboStep == 3 && !_hasTriggeredSecondary && Progression >= 0.78f)
            {
                _hasTriggeredSecondary = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos,
                        SwordDirection * 10f,
                        ModContent.ProjectileType<EternalMoonTidalWash>(),
                        (int)(Projectile.damage * 0.40f),
                        2.5f, Projectile.owner, ai0: -Owner.direction);
                }

                Dust pulse = Dust.NewDustPerfect(tipPos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, EternalMoonVFX.CrescentGlow, 0.3f);
                pulse.customData = new ResonantPulseBehavior(0.045f, 18);
            }

            // === Phase 4 "Full Moon" (Crescendo Tide): Maximum tidal detonation ===
            if (ComboStep == 4 && Progression >= 0.48f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    float baseAngle = SwordDirection.ToRotation();

                    // 6 EternalMoonWave — radial burst
                    for (int i = 0; i < 6; i++)
                    {
                        float spreadAngle = baseAngle + MathHelper.TwoPi * i / 6f * 0.6f - MathHelper.ToRadians(45f);
                        Vector2 vel = spreadAngle.ToRotationVector2() * 17f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<EternalMoonWave>(),
                            (int)(Projectile.damage * 0.60f),
                            Projectile.knockBack * 0.6f, Projectile.owner);
                    }

                    // 4 EternalMoonTidalWash — crosshatch dual pairs
                    for (int pair = 0; pair < 2; pair++)
                    {
                        float angleOffset = MathHelper.ToRadians(14f * (pair - 0.5f));
                        for (int dir = -1; dir <= 1; dir += 2)
                        {
                            Vector2 washVel = SwordDirection.RotatedBy(angleOffset) * (12f + pair * 2f);
                            Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(), tipPos, washVel,
                                ModContent.ProjectileType<EternalMoonTidalWash>(),
                                (int)(Projectile.damage * 0.50f),
                                3f, Projectile.owner, ai0: dir);
                        }
                    }

                    // 3 EternalMoonBeam — homing crystals
                    for (int i = -1; i <= 1; i++)
                    {
                        float beamAngle = baseAngle + MathHelper.ToRadians(22f * i);
                        Vector2 beamVel = beamAngle.ToRotationVector2() * 10f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, beamVel,
                            ModContent.ProjectileType<EternalMoonBeam>(),
                            (int)(Projectile.damage * 1.5f),
                            Projectile.knockBack * 0.3f, Projectile.owner);
                    }

                    // NEW: Tidal Detonation — expanding AoE explosion at blade tip
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos, Vector2.Zero,
                        ModContent.ProjectileType<EternalMoonTidalDetonation>(),
                        (int)(Projectile.damage * 0.80f),
                        Projectile.knockBack * 0.1f, Projectile.owner);
                }

                // Crescendo Finale — full tidal detonation with all VFX
                EternalMoonVFX.CrescendoFinaleVFX(tipPos);

                // Climactic sound — deep tidal crash
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.85f, Pitch = -0.15f }, tipPos);
                SoundEngine.PlaySound(SoundID.Item105 with { Volume = 0.50f, Pitch = -0.2f }, tipPos);
            }

            // Phase 4 secondary: additional radial wash burst
            if (ComboStep == 4 && !_hasTriggeredSecondary && Progression >= 0.72f)
            {
                _hasTriggeredSecondary = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 washVel = SwordDirection.RotatedBy(MathHelper.PiOver4 * i) * 10f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, washVel,
                            ModContent.ProjectileType<EternalMoonTidalWash>(),
                            (int)(Projectile.damage * 0.40f),
                            2f, Projectile.owner, ai0: i);
                    }
                }

                CustomParticles.HaloRing(tipPos, MoonlightSonataPalette.IceBlue, 0.45f, 16);
            }

            // Phase 4 tertiary: ghost reflection at end of spin
            if (ComboStep == 4 && !_hasTriggeredTertiary && Progression >= 0.85f)
            {
                _hasTriggeredTertiary = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 reflectVel = SwordDirection.RotatedBy(MathHelper.Pi * 0.2f) * 7f;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos, reflectVel,
                        ModContent.ProjectileType<EternalMoonReflection>(),
                        (int)(Projectile.damage * 0.25f),
                        Projectile.knockBack * 0.15f, Projectile.owner,
                        ai0: Owner.direction);
                }
            }
        }

        #endregion

        #region Hit Effects

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Apply MusicsDissonance — duration scales with lunar phase
            int debuffTime = 150 + ComboStep * 60;
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), debuffTime);

            // EternalMoon-unique impact: tidal splash + crescent pulse rings + eclipse on crit
            EternalMoonVFX.OnHitImpact(target.Center, ComboStep, hit.Crit);

            // Seeking moonlight crystals on crit
            if (hit.Crit && Main.myPlayer == Projectile.owner)
            {
                int crystalCount = 3 + ComboStep;
                SeekingCrystalHelper.SpawnMoonlightCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    (target.Center - Owner.Center).SafeNormalize(Vector2.UnitY) * 8f,
                    (int)(Projectile.damage * 0.20f),
                    Projectile.knockBack * 0.2f,
                    Projectile.owner,
                    crystalCount);
            }

            // Phase 2+: Extra tidal dust splash
            if (ComboStep >= 2)
            {
                int splashCount = 4 + (ComboStep - 2) * 2;
                for (int i = 0; i < splashCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / splashCount;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                    Color splashColor = EternalMoonVFX.GetLunarPhaseColor((float)i / splashCount, ComboStep);
                    Dust tidal = Dust.NewDustPerfect(target.Center,
                        ModContent.DustType<TidalMoonDust>(),
                        vel, 0, splashColor, 0.3f + ComboStep * 0.03f);
                    tidal.customData = new TidalMoonBehavior(3f, 22);
                }
            }

            // Phase 3+: ResonantPulseDust ring on hit
            if (ComboStep >= 3)
            {
                Dust pulse = Dust.NewDustPerfect(target.Center,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, EternalMoonVFX.CrescentGlow, 0.25f + ComboStep * 0.05f);
                pulse.customData = new ResonantPulseBehavior(0.04f, 16);

                CustomParticles.HaloRing(target.Center, MoonlightSonataPalette.Violet, 0.3f, 14);
            }

            Lighting.AddLight(target.Center, MoonlightSonataPalette.Violet.ToVector3() * (0.6f + ComboStep * 0.15f));
        }

        #endregion

        #region Custom VFX — The Eternal Tide

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression <= 0.08f || Progression >= 0.92f) return;

            float phase = GetMoonPhase(ComboStep);
            Vector2 tipPos = GetBladeTipPosition();

            // === TIDAL WAKE — flowing afterwash from blade positions ===
            EternalMoonVFX.DrawTidalWake(sb, ComboStep, Progression);

            // === LUNAR PHASE AURA — expanding tidal rings around player (phases 3+) ===
            EternalMoonVFX.DrawLunarPhaseAura(sb, Owner.MountedCenter, ComboStep, Progression);

            // === PER-FRAME DUST & PARTICLES ===
            EternalMoonVFX.SwingFrameEffects(
                Owner.MountedCenter, tipPos, SwordDirection, ComboStep, Projectile.timeLeft);

            // === CRESCENT TIP BLOOM (shader-driven on phases 1+, fallback on phase 0) ===
            EternalMoonVFX.DrawCrescentTipBloom(sb, tipPos, SwordRotation, ComboStep, Progression);

            // === SHADER-DRIVEN CRESCENT BLOOM OVERLAY (phases 2+) ===
            if (ComboStep >= 2 && MoonlightSonataShaderManager.HasCrescentBloom)
            {
                DrawShaderCrescentBloom(sb, tipPos, phase);
            }

            // === BLADE-TIP BLOOM ===
            {
                float bloomOpacity = MathHelper.Clamp((Progression - 0.08f) / 0.12f, 0f, 1f)
                                   * MathHelper.Clamp((0.92f - Progression) / 0.12f, 0f, 1f);
                MoonlightVFXLibrary.DrawComboBloom(tipPos, ComboStep, 0.30f + ComboStep * 0.05f, bloomOpacity);
            }

            // === DYNAMIC CRESCENT LIGHT ===
            EternalMoonVFX.AddCrescentLight(tipPos, 0.5f + phase * 0.4f);

            // === PHASE-SPECIFIC ACCENTS ===

            // Phase 1+: LunarMote orbiting blade tip
            if (ComboStep >= 1 && Projectile.timeLeft % (12 - ComboStep) == 0)
            {
                float moteAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Color moteColor = Color.Lerp(EternalMoonVFX.DeepTide,
                    MoonlightSonataPalette.IceBlue, Main.rand.NextFloat());
                Dust mote = Dust.NewDustPerfect(tipPos,
                    ModContent.DustType<LunarMote>(),
                    -SwordDirection * 0.4f,
                    0, moteColor, 0.25f + phase * 0.1f);
                mote.customData = new LunarMoteBehavior(tipPos, moteAngle)
                {
                    OrbitRadius = 12f + ComboStep * 2f,
                    OrbitSpeed = 0.07f + ComboStep * 0.01f,
                    Lifetime = 20 + ComboStep * 2,
                    FadePower = 0.9f
                };
            }

            // Phase 1+: Tidal foam particles trailing along blade
            if (ComboStep >= 1 && Main.rand.NextBool(3 - Math.Min(ComboStep / 2, 1)))
            {
                float bladeT = Main.rand.NextFloat(0.4f, 1f);
                Vector2 foamPos = Vector2.Lerp(Owner.MountedCenter, tipPos, bladeT);
                Color foamColor = Color.Lerp(EternalMoonVFX.TidalFoam,
                    MoonlightSonataPalette.MoonWhite, Main.rand.NextFloat(0.4f));
                Dust star = Dust.NewDustPerfect(foamPos,
                    ModContent.DustType<StarPointDust>(),
                    -SwordDirection * Main.rand.NextFloat(0.5f, 2f),
                    0, foamColor, 0.16f + phase * 0.04f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.1f,
                    Lifetime = 14 + ComboStep * 2,
                    FadeStartTime = 4
                };
            }

            // Phase 2+: Resonant pulse rings from blade arc
            if (ComboStep >= 2 && Projectile.timeLeft % (8 - ComboStep) == 0)
            {
                Vector2 pulsePos = Vector2.Lerp(Owner.MountedCenter, tipPos,
                    Main.rand.NextFloat(0.5f, 1f));
                Color pulseColor = Color.Lerp(EternalMoonVFX.CrescentGlow,
                    EternalMoonVFX.TidalFoam, Main.rand.NextFloat(0.4f));
                Dust pulse = Dust.NewDustPerfect(pulsePos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, pulseColor, 0.12f + ComboStep * 0.02f);
                pulse.customData = new ResonantPulseBehavior(0.03f + ComboStep * 0.005f, 14);
            }

            // Phase 3+: Crescent gold sparkles near blade tip
            if (ComboStep >= 3 && Main.rand.NextBool(2))
            {
                Vector2 sparkPos = tipPos + Main.rand.NextVector2Circular(8f + ComboStep * 2f, 8f + ComboStep * 2f);
                Dust glow = Dust.NewDustPerfect(sparkPos, DustID.PurpleCrystalShard,
                    -SwordDirection * Main.rand.NextFloat(1f, 3f),
                    0, EternalMoonVFX.CrescentGlow, 1.2f + ComboStep * 0.1f);
                glow.noGravity = true;
            }

            // Phase 4: Additional music notes for the grand finale
            if (ComboStep >= 4 && Projectile.timeLeft % 3 == 0)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(tipPos, 1, 6f, 0.7f, 0.9f, 25);
            }
        }

        /// <summary>
        /// Draws the CrescentBloom.fx shader overlay at the blade tip.
        /// Creates a procedural crescent moon shape that grows with the combo phase.
        /// </summary>
        private void DrawShaderCrescentBloom(SpriteBatch sb, Vector2 tipPos, float phase)
        {
            var crescentTex = MoonlightSonataTextures.TidalBloom?.Value
                           ?? MagnumTextureRegistry.GetSoftGlow();
            if (crescentTex == null) return;

            Vector2 drawPos = tipPos - Main.screenPosition;
            Vector2 origin = crescentTex.Size() * 0.5f;
            float bloomScale = (0.15f + phase * 0.25f)
                * (1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.08f);

            float fadeWindow = MathHelper.Clamp((Progression - 0.15f) / 0.15f, 0f, 1f)
                             * MathHelper.Clamp((0.88f - Progression) / 0.12f, 0f, 1f);

            if (fadeWindow < 0.01f) return;

            try
            {
                MoonlightSonataShaderManager.BeginShaderBatch(sb);

                MoonlightSonataShaderManager.ApplyEternalMoonCrescentBloom(
                    Main.GlobalTimeWrappedHourly, phase);

                sb.Draw(crescentTex, drawPos, null,
                    Color.White * fadeWindow, SwordRotation, origin,
                    bloomScale, SpriteEffects.None, 0f);

                MoonlightSonataShaderManager.RestoreDefaultBatch(sb);
            }
            catch
            {
                // Fallback: plain additive bloom if shader fails
                try { MoonlightSonataShaderManager.RestoreDefaultBatch(sb); } catch { }
                sb.Draw(crescentTex, drawPos, null,
                    MoonlightSonataPalette.Additive(MoonlightSonataPalette.Violet, 0.3f * fadeWindow),
                    SwordRotation, origin, bloomScale, SpriteEffects.None, 0f);
            }
        }

        #endregion
    }
}
