using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.Particles.Particle;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Projectiles;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.SandboxLastPrism.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight
{
    /// <summary>
    /// Swing projectile for Incisor of Moonlight — "The Stellar Scalpel".
    /// 5-phase Surgical Precision combo (Precise Incision → Crescent Cut → Constellation Mapping → Harmonic Surge → Stellar Crescendo).
    ///
    /// Attack pattern escalation across the full resonance cycle:
    ///   Phase 0 "Precise Incision" (Opening):   1 wave + 2 star fragments — surgical, minimal VFX
    ///   Phase 1 "Crescent Cut" (Rising):         2 waves + 3 star fragments — constellation trail activates
    ///   Phase 2 "Constellation Mapping" (Lock):  2 waves + 4 star fragments + constellation lock marker
    ///   Phase 3 "Harmonic Surge" (Build):        3 waves + 6 star fragments + harmonic burst AoE
    ///   Phase 4 "Stellar Crescendo" (Climax):    4 waves radial + 8 star fragments spiral + detonation + full VFX
    ///
    /// VFX pipeline (sealed in MeleeSwingBase):
    ///   Trail (Cosmic) → Smear → Blade → Glow → LensFlare → MotionBlur → CustomVFX
    ///
    /// Custom VFX layer adds:
    ///   - Constellation trail (connecting star map drawn by blade tip)
    ///   - ConstellationField.fx shader-driven starfield overlay (phases 2+)
    ///   - Afterimage cascade (ghosted blade positions on Phase 2+)
    ///   - Resonant edge bloom with standing wave pattern
    ///   - IncisorResonance.fx shader-driven trail glow (phases 1+)
    ///   - Custom dust types: LunarMote, StarPointDust, ResonantPulseDust
    ///   - God ray bursts + screen distortion + chromatic aberration on Crescendo
    /// </summary>
    public sealed class IncisorOfMoonlightSwing : MeleeSwingBase
    {
        #region Fields

        private int _crystalCooldown;
        private int _lastComboStep = -1;
        private bool _hasTriggeredSecondary;
        private bool _hasTriggeredTertiary;

        #endregion

        #region Palette

        // Use the canonical palette from MoonlightSonataPalette
        private static readonly Color[] IncisorPalette = MoonlightSonataPalette.IncisorBlade;

        #endregion

        #region Combo Phases — 5-Phase Surgical Precision

        // Phase 0: "Precise Incision" (Opening) — quick horizontal sweep, surgical
        private static readonly ComboPhase Phase0_PreciseIncision = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.9f, 0.2f, 2),
                new CurveSegment(EasingType.PolyIn, 0.2f, -0.7f, 1.5f, 3),
                new CurveSegment(EasingType.PolyOut, 0.82f, 0.8f, 0.12f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.3f,
            duration: 20,
            bladeLength: 148f,
            flip: false,
            squish: 0.94f,
            damageMult: 0.80f
        );

        // Phase 1: "Crescent Cut" (Rising) — reverse diagonal with sharper windup
        private static readonly ComboPhase Phase1_CrescentCut = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, 0.85f, -0.18f, 2),
                new CurveSegment(EasingType.PolyIn, 0.18f, 0.67f, -1.55f, 3),
                new CurveSegment(EasingType.PolyOut, 0.78f, -0.88f, -0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.5f,
            duration: 22,
            bladeLength: 150f,
            flip: true,
            squish: 0.92f,
            damageMult: 0.90f
        );

        // Phase 2: "Constellation Mapping" (Lock) — wide sweep with lock-on marking
        private static readonly ComboPhase Phase2_ConstellationMapping = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1.0f, 0.15f, 2),
                new CurveSegment(EasingType.PolyIn, 0.15f, -0.85f, 1.9f, 4),
                new CurveSegment(EasingType.PolyOut, 0.80f, 1.05f, 0.08f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.7f,
            duration: 26,
            bladeLength: 155f,
            flip: false,
            squish: 0.88f,
            damageMult: 1.05f
        );

        // Phase 3: "Harmonic Surge" (Build) — heavy overhead slam with wind-up pause
        private static readonly ComboPhase Phase3_HarmonicSurge = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, 1.10f, -0.08f, 2),
                new CurveSegment(EasingType.SineIn, 0.10f, 1.02f, -0.06f, 2),
                new CurveSegment(EasingType.PolyIn, 0.18f, 0.96f, -2.10f, 4),
                new CurveSegment(EasingType.PolyOut, 0.82f, -1.14f, -0.08f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 2.0f,
            duration: 32,
            bladeLength: 162f,
            flip: true,
            squish: 0.84f,
            damageMult: 1.25f
        );

        // Phase 4: "Stellar Crescendo" (Climax) — massive spinning slash
        private static readonly ComboPhase Phase4_StellarCrescendo = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1.20f, 0.12f, 2),
                new CurveSegment(EasingType.SineIn, 0.12f, -1.08f, 0.08f, 2),
                new CurveSegment(EasingType.PolyIn, 0.22f, -1.00f, 2.60f, 4),
                new CurveSegment(EasingType.PolyOut, 0.85f, 1.60f, 0.05f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 2.6f,
            duration: 40,
            bladeLength: 172f,
            flip: false,
            squish: 0.78f,
            damageMult: 1.55f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new ComboPhase[]
        {
            Phase0_PreciseIncision,
            Phase1_CrescentCut,
            Phase2_ConstellationMapping,
            Phase3_HarmonicSurge,
            Phase4_StellarCrescendo
        };

        protected override Color[] GetPalette() => IncisorPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Cosmic;

        protected override string GetSmearTexturePath(int comboStep) => comboStep switch
        {
            0 => "MagnumOpus/Assets/Particles/SwordArc2",
            1 => "MagnumOpus/Assets/Particles/SwordArc3",
            2 => "MagnumOpus/Assets/Particles/CurvedSwordSlash",
            3 => "MagnumOpus/Assets/Particles/FlamingArcSwordSlash",
            4 => "MagnumOpus/Assets/Particles/FlamingArcSwordSlash",
            _ => "MagnumOpus/Assets/Particles/SwordArc2"
        };

        #endregion

        #region Virtual Overrides

        protected override SoundStyle GetSwingSound() => ComboStep switch
        {
            0 => SoundID.Item71 with { Pitch = -0.2f, Volume = 0.80f },
            1 => SoundID.Item71 with { Pitch = -0.05f, Volume = 0.85f },
            2 => SoundID.Item71 with { Pitch = 0.1f, Volume = 0.88f },
            3 => SoundID.Item71 with { Pitch = 0.2f, Volume = 0.92f },
            4 => SoundID.Item122 with { Pitch = 0.1f, Volume = 0.95f },
            _ => SoundID.Item71 with { Pitch = -0.1f, Volume = 0.85f }
        };

        protected override int GetInitialDustType() => DustID.PurpleTorch;

        protected override int GetSecondaryDustType() => DustID.Enchanted_Pink;

        protected override Texture2D GetBladeTexture()
            => ModContent.Request<Texture2D>("MagnumOpus/Content/MoonlightSonata/Weapons/IncisorOfMoonlight/IncisorOfMoonlight").Value;

        protected override Vector3 GetLightColor()
        {
            float resonance = GetResonanceLevel(ComboStep);
            float intensity = 0.5f + resonance * 0.5f;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f + ComboStep * 0.9f) * 0.12f;
            Color c = Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.IceBlue,
                Progression * 0.6f + resonance * 0.4f);
            return c.ToVector3() * intensity * pulse;
        }

        /// <summary>Maps combo step to resonance intensity (0-1).</summary>
        private static float GetResonanceLevel(int comboStep) => comboStep switch
        {
            0 => 0.20f,  // Precise Incision — subtle
            1 => 0.40f,  // Crescent Cut
            2 => 0.60f,  // Constellation Mapping
            3 => 0.80f,  // Harmonic Surge
            4 => 1.00f,  // Stellar Crescendo
            _ => 0.5f
        };

        protected override void InitializeSwing()
        {
            base.InitializeSwing();

            IncisorOfMoonlightVFX.ResetSwingTracking();
            _hasTriggeredSecondary = false;
            _hasTriggeredTertiary = false;

            // Phase transition feedback when combo advances
            if (_lastComboStep >= 0 && ComboStep != _lastComboStep)
            {
                IncisorOfMoonlightVFX.PhaseTransitionBurst(Owner.Center, ComboStep);

                // Transition sound — pitch rises with resonance
                SoundEngine.PlaySound(SoundID.Item29 with
                {
                    Pitch = 0.1f + ComboStep * 0.12f,
                    Volume = 0.35f + ComboStep * 0.05f
                }, Owner.Center);

                // Phase 3+: deeper resonance undertone
                if (ComboStep >= 3)
                {
                    SoundEngine.PlaySound(SoundID.Item105 with
                    {
                        Pitch = -0.2f + ComboStep * 0.08f,
                        Volume = 0.25f
                    }, Owner.Center);
                }
            }
            _lastComboStep = ComboStep;
        }

        protected override void DoBehavior_Swinging()
        {
            base.DoBehavior_Swinging();

            // Record afterimages on Phase 2+ for ghosted blade effect
            if (ComboStep >= 2 && Progression > 0.12f && Progression < 0.88f)
            {
                Vector2 tipPos = GetBladeTipPosition();
                IncisorOfMoonlightVFX.RecordAfterimage(Owner.MountedCenter, tipPos, SwordRotation);
            }
        }

        #endregion

        #region Combo Specials — 5-Phase Surgical Precision

        protected override void HandleComboSpecials()
        {
            if (_crystalCooldown > 0) _crystalCooldown--;
            if (hasSpawnedSpecial) return;

            // === Phase 0 "Precise Incision": 1 wave + 2 star fragments ===
            if (ComboStep == 0 && Progression >= 0.65f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    // Crescent wave
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos,
                        SwordDirection * 12f,
                        ModContent.ProjectileType<MoonlightWaveProjectile>(),
                        (int)(Projectile.damage * 0.45f), 2f, Projectile.owner);

                    // 2 star fragments flanking
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 starVel = SwordDirection.RotatedBy(MathHelper.ToRadians(22f * i)) * 9f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, starVel,
                            ModContent.ProjectileType<IncisorStarFragment>(),
                            (int)(Projectile.damage * 0.25f), 1f, Projectile.owner);
                    }
                }

                // Subtle VFX
                CustomParticles.GenericFlare(tipPos, MoonlightVFXLibrary.IceBlue, 0.4f, 12);
                CustomParticles.HaloRing(tipPos, MoonlightVFXLibrary.Violet, 0.25f, 10);
            }

            // === Phase 1 "Crescent Cut": 2 waves + 3 star fragments ===
            if (ComboStep == 1 && Progression >= 0.60f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    // 2 crescent waves with spread
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 waveVel = SwordDirection.RotatedBy(MathHelper.ToRadians(8f * i)) * 13f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, waveVel,
                            ModContent.ProjectileType<MoonlightWaveProjectile>(),
                            (int)(Projectile.damage * 0.50f), 2.5f, Projectile.owner);
                    }

                    // 3 star fragments in fan
                    for (int i = -1; i <= 1; i++)
                    {
                        float angle = MathHelper.ToRadians(18f * i);
                        Vector2 starVel = SwordDirection.RotatedBy(angle) * (9f + Math.Abs(i) * 0.5f);
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, starVel,
                            ModContent.ProjectileType<IncisorStarFragment>(),
                            (int)(Projectile.damage * 0.28f), 1.5f, Projectile.owner);
                    }
                }

                // VFX — dual flare + halo
                CustomParticles.GenericFlare(tipPos, Color.White, 0.55f, 14);
                CustomParticles.GenericFlare(tipPos, MoonlightVFXLibrary.Violet, 0.4f, 12);
                CustomParticles.HaloRing(tipPos, IncisorOfMoonlightVFX.ConstellationBlue, 0.3f, 12);

                // Star point burst
                for (int i = 0; i < 3; i++)
                {
                    float angle = MathHelper.TwoPi * i / 3f;
                    Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 3.5f);
                    Color dustColor = IncisorOfMoonlightVFX.GetResonanceColor((float)i / 3f, 1);
                    Dust star = Dust.NewDustPerfect(tipPos,
                        ModContent.DustType<StarPointDust>(),
                        dustVel, 0, dustColor, 0.3f);
                    star.customData = new StarPointBehavior(0.14f, 18);
                }
            }

            // === Phase 2 "Constellation Mapping": 2 waves + 4 star fragments + constellation lock ===
            if (ComboStep == 2 && Progression >= 0.58f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    // 2 crescent waves — wider spread
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 waveVel = SwordDirection.RotatedBy(MathHelper.ToRadians(12f * i)) * 14f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, waveVel,
                            ModContent.ProjectileType<MoonlightWaveProjectile>(),
                            (int)(Projectile.damage * 0.55f), 2.5f, Projectile.owner);
                    }

                    // 4 star fragments in wider arc
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = MathHelper.ToRadians(-30f + 20f * i);
                        Vector2 starVel = SwordDirection.RotatedBy(angle) * (9f + i * 0.5f);
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, starVel,
                            ModContent.ProjectileType<IncisorStarFragment>(),
                            (int)(Projectile.damage * 0.30f), 2f, Projectile.owner);
                    }

                    // NEW: Constellation Lock — homing marker that attaches to nearest enemy
                    Vector2 lockVel = SwordDirection * 12f;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos, lockVel,
                        ModContent.ProjectileType<IncisorConstellationLock>(),
                        (int)(Projectile.damage * 0.35f),
                        Projectile.knockBack * 0.2f, Projectile.owner);
                }

                // VFX — triple flare + constellation ring
                CustomParticles.GenericFlare(tipPos, Color.White, 0.6f, 16);
                CustomParticles.GenericFlare(tipPos, IncisorOfMoonlightVFX.ConstellationBlue, 0.45f, 14);
                CustomParticles.GenericFlare(tipPos, MoonlightVFXLibrary.Violet, 0.35f, 12);
                CustomParticles.HaloRing(tipPos, IncisorOfMoonlightVFX.ConstellationBlue, 0.35f, 14);

                // Constellation dust burst
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                    Color dustColor = IncisorOfMoonlightVFX.GetResonanceColor((float)i / 6f, 2);
                    Dust star = Dust.NewDustPerfect(tipPos,
                        ModContent.DustType<StarPointDust>(),
                        dustVel, 0, dustColor, 0.35f);
                    star.customData = new StarPointBehavior(0.16f, 22);
                }

                MoonlightVFXLibrary.SpawnMusicNotes(tipPos, 3, 25f, 0.8f, 1.0f, 30);
            }

            // Phase 2 secondary: additional star burst at later progression
            if (ComboStep == 2 && !_hasTriggeredSecondary && Progression >= 0.82f)
            {
                _hasTriggeredSecondary = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = MathHelper.ToRadians(-20f + 20f * i);
                        Vector2 vel = SwordDirection.RotatedBy(angle) * 7f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<IncisorStarFragment>(),
                            (int)(Projectile.damage * 0.22f), 1f, Projectile.owner);
                    }
                }

                Dust pulse = Dust.NewDustPerfect(tipPos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, IncisorOfMoonlightVFX.FrequencyPulse, 0.3f);
                pulse.customData = new ResonantPulseBehavior(0.04f, 18);
            }

            // === Phase 3 "Harmonic Surge": 3 waves + 6 star fragments + harmonic burst ===
            if (ComboStep == 3 && Progression >= 0.52f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    float baseAngle = SwordDirection.ToRotation();

                    // 3 crescent waves in wide fan
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 waveVel = SwordDirection.RotatedBy(MathHelper.ToRadians(14f * i)) * 15f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, waveVel,
                            ModContent.ProjectileType<MoonlightWaveProjectile>(),
                            (int)(Projectile.damage * 0.55f), 3f, Projectile.owner);
                    }

                    // 6 star fragments in spread pattern
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.ToRadians(-40f + 16f * i);
                        Vector2 starVel = SwordDirection.RotatedBy(angle) * (8f + Main.rand.NextFloat(1f));
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, starVel,
                            ModContent.ProjectileType<IncisorStarFragment>(),
                            (int)(Projectile.damage * 0.30f), 2f, Projectile.owner);
                    }

                    // NEW: Harmonic Burst — expanding resonant detonation
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos, Vector2.Zero,
                        ModContent.ProjectileType<IncisorHarmonicBurst>(),
                        (int)(Projectile.damage * 0.60f),
                        Projectile.knockBack * 0.3f, Projectile.owner);
                }

                // Heavy impact VFX
                CustomParticles.GenericFlare(tipPos, Color.White, 0.7f, 18);
                CustomParticles.GenericFlare(tipPos, IncisorOfMoonlightVFX.FrequencyPulse, 0.55f, 16);
                CustomParticles.GenericFlare(tipPos, MoonlightVFXLibrary.IceBlue, 0.4f, 14);
                CustomParticles.HaloRing(tipPos, IncisorOfMoonlightVFX.HarmonicWhite, 0.4f, 16);

                // Screen distortion on release
                if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
                {
                    ScreenDistortionManager.TriggerRipple(tipPos, IncisorOfMoonlightVFX.FrequencyPulse, 0.35f, 16);
                    MagnumScreenEffects.AddScreenShake(3f);
                }

                // Star point cascade
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 5f);
                    Color dustColor = IncisorOfMoonlightVFX.GetResonanceColor((float)i / 8f, 3);
                    Dust star = Dust.NewDustPerfect(tipPos,
                        ModContent.DustType<StarPointDust>(),
                        dustVel, 0, dustColor, 0.4f);
                    star.customData = new StarPointBehavior(0.18f, 25);
                }

                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.6f, Pitch = 0.05f }, tipPos);
            }

            // Phase 3 secondary: additional star fragments at later progression
            if (ComboStep == 3 && !_hasTriggeredSecondary && Progression >= 0.78f)
            {
                _hasTriggeredSecondary = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 vel = SwordDirection.RotatedBy(MathHelper.ToRadians(25f * i)) * 8f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<IncisorStarFragment>(),
                            (int)(Projectile.damage * 0.25f), 1.5f, Projectile.owner);
                    }
                }

                Dust pulse = Dust.NewDustPerfect(tipPos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, IncisorOfMoonlightVFX.HarmonicWhite, 0.3f);
                pulse.customData = new ResonantPulseBehavior(0.045f, 18);
            }

            // === Phase 4 "Stellar Crescendo": Maximum resonance detonation ===
            if (ComboStep == 4 && Progression >= 0.48f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    float baseAngle = SwordDirection.ToRotation();

                    // 4 crescent waves — radial spread
                    for (int i = 0; i < 4; i++)
                    {
                        float spreadAngle = baseAngle + MathHelper.TwoPi * i / 4f * 0.5f - MathHelper.ToRadians(35f);
                        Vector2 vel = spreadAngle.ToRotationVector2() * 16f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<MoonlightWaveProjectile>(),
                            (int)(Projectile.damage * 0.60f),
                            Projectile.knockBack * 0.6f, Projectile.owner);
                    }

                    // 8 star fragments in spiral burst
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f;
                        Vector2 starVel = angle.ToRotationVector2() * (6f + Main.rand.NextFloat(2f));
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, starVel,
                            ModContent.ProjectileType<IncisorStarFragment>(),
                            (int)(Projectile.damage * 0.30f), 2f, Projectile.owner);
                    }

                    // Lunar detonation zone at tip
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos, Vector2.Zero,
                        ModContent.ProjectileType<IncisorLunarDetonation>(),
                        (int)(Projectile.damage * 0.80f), 4f, Projectile.owner,
                        ai0: 0f, ai1: 200f);

                    // Constellation Lock — second lock for bonus targeting
                    Vector2 lockVel = SwordDirection.RotatedBy(MathHelper.PiOver4 * -Owner.direction) * 10f;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos, lockVel,
                        ModContent.ProjectileType<IncisorConstellationLock>(),
                        (int)(Projectile.damage * 0.30f),
                        2f, Projectile.owner);
                }

                // Crescendo finisher — the night sky trembles
                IncisorOfMoonlightVFX.CrescendoFinisherVFX(tipPos);

                // Climactic sound
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.85f, Pitch = 0.15f }, tipPos);
                SoundEngine.PlaySound(SoundID.Item105 with { Volume = 0.45f, Pitch = -0.1f }, tipPos);
            }

            // Phase 4 secondary: additional radial star burst
            if (ComboStep == 4 && !_hasTriggeredSecondary && Progression >= 0.72f)
            {
                _hasTriggeredSecondary = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 4f + MathHelper.PiOver4;
                        Vector2 vel = angle.ToRotationVector2() * 7f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<IncisorStarFragment>(),
                            (int)(Projectile.damage * 0.22f), 1f, Projectile.owner);
                    }
                }

                CustomParticles.HaloRing(tipPos, IncisorOfMoonlightVFX.ConstellationBlue, 0.45f, 16);
            }

            // Phase 4 tertiary: constellation lock at end of spin
            if (ComboStep == 4 && !_hasTriggeredTertiary && Progression >= 0.86f)
            {
                _hasTriggeredTertiary = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 lockVel = SwordDirection * 10f;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos, lockVel,
                        ModContent.ProjectileType<IncisorConstellationLock>(),
                        (int)(Projectile.damage * 0.25f),
                        1.5f, Projectile.owner);
                }
            }
        }

        #endregion

        #region Hit Effects

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Apply MusicsDissonance — duration scales with resonance
            int debuffTime = 150 + ComboStep * 55;
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), debuffTime);

            // Incisor-unique impact: resonant shockwave with tuning-fork pattern
            IncisorOfMoonlightVFX.OnHitImpact(target.Center, ComboStep, hit.Crit);

            // Seeking crystals on hit (25-frame cooldown)
            if (_crystalCooldown <= 0)
            {
                int crystalCount = hit.Crit ? 5 : 3;
                if (Main.myPlayer == Projectile.owner)
                {
                    SeekingCrystalHelper.SpawnMoonlightCrystals(
                        Projectile.GetSource_FromThis(),
                        target.Center,
                        (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX) * 7f,
                        (int)(Projectile.damage * 0.30f),
                        2.5f,
                        Projectile.owner,
                        crystalCount);
                }
                _crystalCooldown = 25;
            }

            // Crit bonus: spawn homing star fragments
            if (hit.Crit && Main.myPlayer == Projectile.owner)
            {
                int critStars = 2 + ComboStep / 2;
                for (int i = 0; i < critStars; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 vel = angle.ToRotationVector2() * 6f;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), target.Center, vel,
                        ModContent.ProjectileType<IncisorStarFragment>(),
                        (int)(Projectile.damage * 0.22f), 1f, Projectile.owner);
                }
            }

            // Phase 2+: Extra star point dust splash
            if (ComboStep >= 2)
            {
                int splashCount = 3 + (ComboStep - 2) * 2;
                for (int i = 0; i < splashCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / splashCount;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                    Color splashColor = IncisorOfMoonlightVFX.GetResonanceColor((float)i / splashCount, ComboStep);
                    Dust star = Dust.NewDustPerfect(target.Center,
                        ModContent.DustType<StarPointDust>(),
                        vel, 0, splashColor, 0.35f + ComboStep * 0.03f);
                    star.customData = new StarPointBehavior(0.15f, 22);
                }
            }

            // Phase 3+: ResonantPulseDust ring on hit
            if (ComboStep >= 3)
            {
                Dust pulse = Dust.NewDustPerfect(target.Center,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, IncisorOfMoonlightVFX.HarmonicWhite, 0.25f + ComboStep * 0.05f);
                pulse.customData = new ResonantPulseBehavior(0.04f, 16);

                CustomParticles.HaloRing(target.Center, IncisorOfMoonlightVFX.FrequencyPulse, 0.3f, 14);
            }

            Lighting.AddLight(target.Center, MoonlightVFXLibrary.Violet.ToVector3() * (0.6f + ComboStep * 0.15f));
        }

        #endregion

        #region Custom VFX — The Stellar Scalpel

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression <= 0.08f || Progression >= 0.92f) return;

            float resonance = GetResonanceLevel(ComboStep);
            Vector2 tipPos = GetBladeTipPosition();

            // === CONSTELLATION TRAIL ===
            IncisorOfMoonlightVFX.DrawConstellationTrail(sb, ComboStep, Progression);

            // === CONSTELLATION FIELD OVERLAY (phases 2+, shader-driven) ===
            IncisorOfMoonlightVFX.DrawConstellationFieldOverlay(sb, Owner.MountedCenter, tipPos, ComboStep, Progression);

            // === AFTERIMAGE CASCADE (Phase 2+) ===
            if (ComboStep >= 2)
            {
                Texture2D bladeTex = GetBladeTexture();
                if (bladeTex != null)
                {
                    float bladeScale = CurrentPhase.BladeLength / bladeTex.Width;
                    IncisorOfMoonlightVFX.DrawAfterimages(sb, bladeTex, bladeScale);
                }
            }

            // === RESONANT EDGE BLOOM ===
            IncisorOfMoonlightVFX.DrawResonantEdgeBloom(
                sb, Owner.MountedCenter, tipPos, ComboStep, Progression);

            // === PER-FRAME DUST & PARTICLES ===
            IncisorOfMoonlightVFX.SwingFrameEffects(
                Owner.MountedCenter, tipPos, SwordDirection, ComboStep, Projectile.timeLeft);

            // === SHADER-DRIVEN RESONANCE GLOW OVERLAY (phases 1+) ===
            if (ComboStep >= 1 && MoonlightSonataShaderManager.HasIncisorResonance)
            {
                DrawShaderResonanceGlow(sb, tipPos, resonance);
            }

            // === BLADE-TIP BLOOM ===
            {
                float bloomOpacity = MathHelper.Clamp((Progression - 0.08f) / 0.12f, 0f, 1f)
                                   * MathHelper.Clamp((0.92f - Progression) / 0.12f, 0f, 1f);
                MoonlightVFXLibrary.DrawComboBloom(tipPos, ComboStep, 0.35f + ComboStep * 0.05f, bloomOpacity);
            }

            // === RESONANT LIGHT ===
            IncisorOfMoonlightVFX.AddResonantLight(tipPos, 0.5f + ComboStep * 0.12f);

            // === PHASE-SPECIFIC ACCENTS ===

            // Phase 1+: LunarMote orbiting blade tip (increased frequency with combo)
            if (ComboStep >= 1 && Projectile.timeLeft % (11 - ComboStep) == 0)
            {
                float moteAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Color moteColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple,
                    MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust mote = Dust.NewDustPerfect(tipPos,
                    ModContent.DustType<LunarMote>(),
                    -SwordDirection * 0.3f,
                    0, moteColor, 0.28f + resonance * 0.08f);
                mote.customData = new LunarMoteBehavior(tipPos, moteAngle)
                {
                    OrbitRadius = 10f + ComboStep * 2f,
                    OrbitSpeed = 0.08f + ComboStep * 0.01f,
                    Lifetime = 18 + ComboStep * 2,
                    FadePower = 0.9f
                };
            }

            // Phase 1+: StarPointDust trailing along blade
            if (ComboStep >= 1 && Main.rand.NextBool(3 - Math.Min(ComboStep / 2, 1)))
            {
                float bladeT = Main.rand.NextFloat(0.4f, 1f);
                Vector2 bladePos = Vector2.Lerp(Owner.MountedCenter, tipPos, bladeT);
                Color starColor = IncisorOfMoonlightVFX.GetResonanceColor(bladeT, ComboStep);
                Dust star = Dust.NewDustPerfect(bladePos,
                    ModContent.DustType<StarPointDust>(),
                    -SwordDirection * Main.rand.NextFloat(0.5f, 2f),
                    0, starColor, 0.18f + resonance * 0.04f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.14f,
                    Lifetime = 14 + ComboStep * 2,
                    FadeStartTime = 4
                };
            }

            // Phase 2+: Crystal shards trailing behind blade
            if (ComboStep >= 2 && Main.rand.NextBool(3))
            {
                float bladeT = Main.rand.NextFloat(0.4f, 1f);
                Vector2 bladePos = Vector2.Lerp(Owner.MountedCenter, tipPos, bladeT);
                Color shardColor = IncisorOfMoonlightVFX.GetResonanceColor(bladeT, ComboStep);
                Dust glow = Dust.NewDustPerfect(bladePos, DustID.PurpleCrystalShard,
                    -SwordDirection * Main.rand.NextFloat(1f, 3f), 0, shardColor, 1.3f + ComboStep * 0.1f);
                glow.noGravity = true;
            }

            // Phase 2+: Resonant pulse rings from blade arc
            if (ComboStep >= 2 && Projectile.timeLeft % (8 - ComboStep) == 0)
            {
                Vector2 pulsePos = Vector2.Lerp(Owner.MountedCenter, tipPos,
                    Main.rand.NextFloat(0.5f, 1f));
                Color pulseColor = Color.Lerp(IncisorOfMoonlightVFX.FrequencyPulse,
                    IncisorOfMoonlightVFX.HarmonicWhite, Main.rand.NextFloat(0.3f));
                Dust pulse = Dust.NewDustPerfect(pulsePos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, pulseColor, 0.12f + ComboStep * 0.02f);
                pulse.customData = new ResonantPulseBehavior(0.03f + ComboStep * 0.005f, 14);
            }

            // Phase 3+: Harmonic sparkles near blade tip
            if (ComboStep >= 3 && Main.rand.NextBool(2))
            {
                Vector2 sparkPos = tipPos + Main.rand.NextVector2Circular(8f + ComboStep * 2f, 8f + ComboStep * 2f);
                Dust glow = Dust.NewDustPerfect(sparkPos, DustID.PurpleCrystalShard,
                    -SwordDirection * Main.rand.NextFloat(1f, 3f),
                    0, IncisorOfMoonlightVFX.HarmonicWhite, 1.2f + ComboStep * 0.1f);
                glow.noGravity = true;
            }

            // Phase 4: Music notes for the grand finale
            if (ComboStep >= 4 && Projectile.timeLeft % 3 == 0)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(tipPos, 1, 6f, 0.7f, 0.9f, 25);
            }
        }

        /// <summary>
        /// Draws the IncisorResonance.fx shader glow overlay at the blade tip.
        /// Creates a resonant standing-wave glow that intensifies with combo phase.
        /// </summary>
        private void DrawShaderResonanceGlow(SpriteBatch sb, Vector2 tipPos, float resonance)
        {
            var glowTex = MoonlightSonataTextures.TuningForkFlare?.Value
                       ?? MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 drawPos = tipPos - Main.screenPosition;
            Vector2 origin = glowTex.Size() * 0.5f;
            float glowScale = (0.12f + resonance * 0.22f)
                * (1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.08f);

            float fadeWindow = MathHelper.Clamp((Progression - 0.12f) / 0.15f, 0f, 1f)
                             * MathHelper.Clamp((0.88f - Progression) / 0.12f, 0f, 1f);

            if (fadeWindow < 0.01f) return;

            try
            {
                MoonlightSonataShaderManager.BeginShaderBatch(sb);

                MoonlightSonataShaderManager.ApplyIncisorResonanceTrail(
                    Main.GlobalTimeWrappedHourly, resonance, glowPass: true);

                sb.Draw(glowTex, drawPos, null,
                    Color.White * fadeWindow, SwordRotation, origin,
                    glowScale, SpriteEffects.None, 0f);

                MoonlightSonataShaderManager.RestoreDefaultBatch(sb);
            }
            catch
            {
                // Fallback: plain additive bloom if shader fails
                try { MoonlightSonataShaderManager.RestoreDefaultBatch(sb); } catch { }
                sb.Draw(glowTex, drawPos, null,
                    MoonlightSonataPalette.Additive(MoonlightSonataPalette.Violet, 0.3f * fadeWindow),
                    SwordRotation, origin, glowScale, SpriteEffects.None, 0f);
            }
        }

        #endregion
    }
}
