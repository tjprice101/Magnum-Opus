using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Buffs;
using MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Dusts;
using MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Particles;
using MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Primitives;
using MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Shaders;
using MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Utilities;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Common.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Utilities.SakuraUtils;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom
{
    /// <summary>
    /// SakurasBlossomSwing — "The Petals That Remember"
    /// 
    /// 4-phase sakura combo: each phase is a movement in a spring symphony.
    ///   Phase 0: Petal Slash       — Quick opener, 1 spectral copy
    ///   Phase 1: Crimson Scatter   — Backhand scatter, 2 spectral copies
    ///   Phase 2: Blossom Bloom     — Rising bloom arc, 3 spectral copies
    ///   Phase 3: Storm of Petals   — Devastating finisher, 4 spectral copies + sakura storm
    ///
    /// Special: Petal Dash — Charge through enemies scattering petals.
    ///          Empowered Storm — After dash connects, next swing is empowered Phase 3.
    ///
    /// Visual Layers (render order):
    ///   1. Sakura Glow Pass   — Wide soft pink-gold bloom underlayer via SakuraTrailGlow
    ///   2. Sakura Trail Pass  — Core arc trail via SakuraTrailFlow with petal gradient
    ///   3. Dash Trail         — During dash: trailing positions with petal glow
    ///   4. Blade Sprite       — UV-rotated blade + cross flare at tip
    ///   5. Petal Bloom        — Soft bloom overlay at blade tip (Phase 1+)
    /// </summary>
    public sealed class SakurasBlossomSwing : ModProjectile
    {
        #region Constants and Properties

        public Player Owner => Main.player[Projectile.owner];

        private const float BladeLength = 160f;
        private const int BaseSwingTime = 72;
        private const float MaxSwingAngle = MathHelper.PiOver2 * 1.8f;
        private const float DashSpeed = 42f;
        private const float DashPercentage = 0.55f;
        private const float EmpoweredUpscale = 1.35f;
        private const float ReboundSpeed = 5f;
        private const int DashCooldown = 60 * 3;
        private const int EmpoweredOpportunity = 37 * 3;
        private const float SubProjectileDamagePenalty = 0.3f;
        private const float FinisherDamageFactor = 1.7f;

        public int GetSwingTime
        {
            get
            {
                if (State == SwingState.PetalDash)
                    return SakurasBlossom.PetalDashTime * Projectile.extraUpdates;
                // Phase-based timing: longer phases for later combo steps
                return _comboStep switch
                {
                    0 => 60,
                    1 => 66,
                    2 => 78,
                    3 => 96,
                    _ => BaseSwingTime
                };
            }
        }

        public float Timer => SwingTime - Projectile.timeLeft;
        public float Progression => Timer / (float)SwingTime;

        public float DashProgression => Progression < (1 - DashPercentage)
            ? 0 : (Progression - (1 - DashPercentage)) / DashPercentage;

        #endregion

        #region State Machine

        public enum SwingState
        {
            Swinging,
            PetalDash
        }

        public SwingState State
        {
            get => Projectile.ai[0] == 1 ? SwingState.PetalDash : SwingState.Swinging;
            set => Projectile.ai[0] = (int)value;
        }

        public bool PerformingEmpoweredFinale => Projectile.ai[0] > 1;

        public bool InPostDashStasis
        {
            get => Projectile.ai[1] > 0;
            set => Projectile.ai[1] = value ? 1 : 0;
        }

        public ref float SwingTime => ref Projectile.localAI[0];
        public ref float SquishFactor => ref Projectile.localAI[1];

        private int _comboStep;
        private float _phaseIntensity;

        public float IdealSize => PerformingEmpoweredFinale ? EmpoweredUpscale : 1f;

        #endregion

        #region Swing Animation Curves

        public int Direction => Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;
        public float BaseRotation => Projectile.velocity.ToRotation();
        public Vector2 SquishVector => new Vector2(1f + (1 - SquishFactor) * 0.5f, SquishFactor);

        // Phase 0: Petal Slash — quick horizontal opener, petals scatter
        public CurveSegment Phase0_Windup = new(PolyOutEasing, 0f, -0.85f, 0.16f, 2);
        public CurveSegment Phase0_Swing = new(PolyInEasing, 0.16f, -0.69f, 1.50f, 3);
        public CurveSegment Phase0_Settle = new(PolyOutEasing, 0.78f, 0.81f, 0.10f, 2);

        // Phase 1: Crimson Scatter — backhand that tosses spectral copies wide
        public CurveSegment Phase1_Windup = new(PolyOutEasing, 0f, 0.92f, -0.20f, 2);
        public CurveSegment Phase1_Swing = new(PolyInEasing, 0.20f, 0.72f, -1.65f, 3);
        public CurveSegment Phase1_Settle = new(PolyOutEasing, 0.82f, -0.93f, -0.08f, 2);

        // Phase 2: Blossom Bloom — rising arc, pollen explodes from blade
        public CurveSegment Phase2_Windup = new(SineOutEasing, 0f, -1.0f, 0.22f, 2);
        public CurveSegment Phase2_Swing = new(PolyInEasing, 0.22f, -0.78f, 1.85f, 3);
        public CurveSegment Phase2_Settle = new(PolyOutEasing, 0.84f, 1.07f, 0.06f, 2);

        // Phase 3: Storm of Petals — massive slam, sakura storm erupts (power-4 accel)
        public CurveSegment Phase3_Windup = new(SineOutEasing, 0f, -1.18f, 0.16f, 2);
        public CurveSegment Phase3_Swing = new(PolyInEasing, 0.18f, -1.02f, 2.30f, 4);
        public CurveSegment Phase3_Settle = new(PolyOutEasing, 0.82f, 1.28f, 0.04f, 2);

        public float SwingAngleShiftAtProgress(float progress)
        {
            if (State == SwingState.PetalDash) return 0;

            float p;
            switch (_comboStep)
            {
                case 1:
                    p = PiecewiseAnimation(progress, Phase1_Windup, Phase1_Swing, Phase1_Settle);
                    return MaxSwingAngle * 1.1f * p;
                case 2:
                    p = PiecewiseAnimation(progress, Phase2_Windup, Phase2_Swing, Phase2_Settle);
                    return MaxSwingAngle * 1.3f * p;
                case 3:
                    p = PiecewiseAnimation(progress, Phase3_Windup, Phase3_Swing, Phase3_Settle);
                    return MaxSwingAngle * 1.55f * p;
                default:
                    p = PiecewiseAnimation(progress, Phase0_Windup, Phase0_Swing, Phase0_Settle);
                    return MaxSwingAngle * 0.9f * p;
            }
        }

        public float SwordRotationAtProgress(float progress) =>
            State == SwingState.PetalDash ? BaseRotation :
            BaseRotation + SwingAngleShiftAtProgress(progress) * Direction * (_comboStep == 1 ? -1 : 1);

        public float SquishAtProgress(float progress) =>
            State == SwingState.PetalDash ? 1 :
            MathHelper.Lerp(SquishVector.X, SquishVector.Y,
                (float)Math.Abs(Math.Sin(SwingAngleShiftAtProgress(progress))));

        public Vector2 DirectionAtProgress(float progress) =>
            State == SwingState.PetalDash ? Projectile.velocity :
            SwordRotationAtProgress(progress).ToRotationVector2() * SquishAtProgress(progress);

        public float SwingAngleShift => SwingAngleShiftAtProgress(Progression);
        public float SwordRotation => SwordRotationAtProgress(Progression);
        public float CurrentSquish => SquishAtProgress(Progression);
        public Vector2 SwordDirection => DirectionAtProgress(Progression);

        #endregion

        #region Trail Data

        public float TrailEndProgression
        {
            get
            {
                float endProg;
                if (Progression < 0.7f)
                    endProg = Progression - 0.45f + 0.1f * (Progression / 0.7f);
                else
                    endProg = Progression - 0.35f * (1 - (Progression - 0.7f) / 0.7f);
                return Math.Clamp(endProg, 0, 1);
            }
        }

        public float RealProgressionAtTrailCompletion(float completion) =>
            MathHelper.Lerp(Progression, TrailEndProgression, completion);

        public Vector2 DirectionAtProgressSmoothed(float progress)
        {
            float angleShift = SwingAngleShiftAtProgress(progress);
            Vector2 anglePoint = angleShift.ToRotationVector2();
            anglePoint.X *= SquishVector.X;
            anglePoint.Y *= SquishVector.Y;
            angleShift = anglePoint.ToRotation();
            return (BaseRotation + angleShift * Direction * (_comboStep == 1 ? -1 : 1)).ToRotationVector2()
                * SquishAtProgress(progress);
        }

        // Dash displacement curves
        public CurveSegment DashWindback = new(SineBumpEasing, 0f, -6f, -10f);
        public CurveSegment DashThrust => new(PolyOutEasing, 1 - DashPercentage, -6, 8f, 4);
        public float DashDisplace => PiecewiseAnimation(Progression, DashWindback, DashThrust);

        #endregion

        #region Particle and Dust Density

        public float DustRisk
        {
            get
            {
                if (Progression > 0.85f) return 0;
                if (Progression < 0.35f) return (float)Math.Pow(Progression / 0.35f, 2) * 0.15f;
                if (Progression < 0.5f) return 0.15f + 0.75f * (Progression - 0.35f) / 0.15f;
                return 0.9f * _phaseIntensity;
            }
        }

        #endregion

        #region Texture References

        public override string Texture => "MagnumOpus/Content/Eroica/Weapons/SakurasBlossom/SakurasBlossom";
        private static Asset<Texture2D> _lensFlare;
        private static Asset<Texture2D> _bloomCircle;
        private static Asset<Texture2D> _noiseTexture;

        #endregion

        #region Setup

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 80;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 90;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.MaxUpdates = 3;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 8;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(SwingTime);
            writer.Write(SquishFactor);
            writer.Write(_comboStep);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            SwingTime = reader.ReadSingle();
            SquishFactor = reader.ReadSingle();
            _comboStep = reader.ReadInt32();
        }

        #endregion

        #region Collision

        public override bool ShouldUpdatePosition() => State == SwingState.PetalDash && !InPostDashStasis;

        public override bool? CanDamage()
        {
            if (State != SwingState.PetalDash) return null;
            if (InPostDashStasis) return false;
            if (Projectile.timeLeft > SwingTime * DashPercentage) return false;
            return null;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Projectile.Center;
            Vector2 end = start + SwordDirection * (BladeLength + 40) * Projectile.scale;
            float width = State == SwingState.PetalDash ? Projectile.scale * 42f : Projectile.scale * 28f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref _);
        }

        #endregion

        #region Initialization

        public void InitializationEffects(bool startInit)
        {
            Projectile.velocity = Owner.MountedCenter.DirectionTo(Main.MouseWorld);
            SquishFactor = Main.rand.NextFloat(0.7f, 1f);

            _comboStep = Owner.SakuraBlossom().ComboStep;
            _phaseIntensity = 0.3f + _comboStep * 0.233f; // 0.3, 0.53, 0.77, 1.0

            if (startInit && State != SwingState.PetalDash)
                Projectile.scale = 0.02f;
            else
            {
                Projectile.scale = 1f;
                if (PerformingEmpoweredFinale)
                {
                    State = SwingState.Swinging;
                    _comboStep = 3;
                    _phaseIntensity = 1f;
                }
            }

            if (PerformingEmpoweredFinale)
                SquishFactor = 0.72f;

            SwingTime = GetSwingTime;
            Projectile.timeLeft = (int)SwingTime;
            Projectile.netUpdate = true;
        }

        #endregion

        #region AI

        public override void AI()
        {
            if (InPostDashStasis || Projectile.timeLeft == 0)
                return;

            if (Projectile.timeLeft >= 9999 || (Projectile.timeLeft == 1 && Owner.channel && State != SwingState.PetalDash))
                InitializationEffects(Projectile.timeLeft >= 9999);

            switch (State)
            {
                case SwingState.Swinging:
                    DoBehavior_Swinging();
                    break;
                case SwingState.PetalDash:
                    DoBehavior_PetalDash();
                    break;
            }

            // Anchor to owner
            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            Owner.heldProj = Projectile.whoAmI;
            Owner.SetDummyItemTime(2);
            Owner.ChangeDir(Direction);

            // Arm rotation
            float armRotation = SwordRotation - MathHelper.PiOver2;
            Owner.SetCompositeArmFront(Math.Abs(armRotation) > 0.01f, Player.CompositeArmStretchAmount.Full, armRotation);

            // Dash cooldown freeze
            if (Projectile.timeLeft == 1 && State == SwingState.PetalDash && !InPostDashStasis)
            {
                Projectile.timeLeft = DashCooldown;
                InPostDashStasis = true;
                Owner.fullRotation = 0f;
                Owner.SakuraBlossom().IsLunging = false;
            }
        }

        #endregion

        #region Swing Behavior

        public void DoBehavior_Swinging()
        {
            // Play swing sound at 20% with phase-escalating pitch
            if (Projectile.timeLeft == (int)(SwingTime / 5))
            {
                SoundEngine.PlaySound(SoundID.Item71 with
                {
                    Volume = 0.85f + _comboStep * 0.05f,
                    Pitch = -0.22f + _comboStep * 0.18f,
                    PitchVariance = 0.2f
                }, Projectile.Center);

                // Finisher chord for Phase 3
                if (_comboStep == 3 || PerformingEmpoweredFinale)
                    SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.5f, Pitch = -0.3f }, Projectile.Center);
            }

            // Dynamic sakura lighting along the blade
            Vector3 lightColor = Color.Lerp(SakuraPink, GoldenPollen, (float)Math.Pow(Progression, 2)).ToVector3();
            lightColor *= 1.1f * _phaseIntensity * (float)Math.Sin(Progression * MathHelper.Pi);
            Lighting.AddLight(Owner.MountedCenter + SwordDirection * 90, lightColor);

            // Scale up to ideal size
            if (Projectile.scale < IdealSize)
                Projectile.scale = MathHelper.Lerp(Projectile.scale, IdealSize, 0.08f);

            // Shrink near end of slash
            if (!Owner.channel && Progression > 0.7f)
                Projectile.scale = (0.5f + 0.5f * (float)Math.Pow(1 - (Progression - 0.7f) / 0.3f, 0.5)) * IdealSize;

            // === DUST SPAWNING ===
            // Petal dust from blade edge
            if (Main.rand.NextFloat() * 3f < DustRisk)
            {
                Vector2 dustPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale *
                    (float)Math.Pow(Main.rand.NextFloat(0.5f, 1f), 0.5f);
                Dust d = Dust.NewDustPerfect(dustPos, ModContent.DustType<SakuraPetalDust>(),
                    SwordDirection.RotatedBy(-MathHelper.PiOver2 * Direction) * 2f);
                d.noGravity = true;
                d.alpha = 10;
                d.scale = 0.5f * _phaseIntensity;
            }

            // Sakura pink / golden pollen ambient dust
            if (Main.rand.NextFloat() < DustRisk * 0.7f)
            {
                Color dustColor = Color.Lerp(SakuraPink, GoldenPollen, Main.rand.NextFloat());
                Vector2 dustPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale *
                    (float)Math.Pow(Main.rand.NextFloat(0.2f, 1f), 0.5f);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.PinkTorch,
                    SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction) * 2.2f, 0, dustColor);
                d.scale = 0.4f;
                d.fadeIn = Main.rand.NextFloat() * 1.0f;
                d.noGravity = true;
            }

            // === PARTICLE SPAWNING ===
            SpawnSwingParticles();

            // === SPECTRAL COPY PROJECTILES ===
            SpawnComboProjectiles();

            // Advance combo on first swing frame
            if (Timer == 1)
                Owner.SakuraBlossom().AdvanceCombo();
        }

        private void SpawnSwingParticles()
        {
            if (Main.dedServ) return;

            float tipX = BladeLength * Projectile.scale;
            Vector2 tipPos = Owner.MountedCenter + SwordDirection * tipX;

            // Sakura petals drifting from blade — density scales with combo step
            if (Main.rand.NextFloat() < 0.35f * _phaseIntensity && Progression > 0.2f && Progression < 0.85f)
            {
                Vector2 petalVel = SwordDirection.RotatedByRandom(0.6f) * Main.rand.NextFloat(0.5f, 2.5f)
                    + new Vector2(0, Main.rand.NextFloat(0.3f, 1f)); // gentle drift down
                Color petalColor = GetPetalGradient(Main.rand.NextFloat());
                SakuraParticleHandler.SpawnParticle(new SakuraPetalParticle(
                    tipPos + Main.rand.NextVector2Circular(15f, 15f), petalVel,
                    petalColor, Main.rand.NextFloat(0.4f, 0.9f) * _phaseIntensity,
                    Main.rand.Next(30, 60)));
            }

            // Blossom sparks on fast part of swing (Phase 1+)
            if (_comboStep >= 1 && Progression > 0.4f && Progression < 0.8f && Main.rand.NextBool(4))
            {
                Vector2 sparkVel = SwordDirection.RotatedByRandom(0.3f) * Main.rand.NextFloat(4f, 8f);
                SakuraParticleHandler.SpawnParticle(new BlossomSparkParticle(
                    tipPos, sparkVel, Color.Lerp(SpringWhite, SakuraPink, 0.3f),
                    Main.rand.NextFloat(0.4f, 0.8f), Main.rand.Next(15, 25)));
            }

            // Pollen motes rising (Phase 2+) — golden warmth
            if (_comboStep >= 2 && Progression > 0.3f && Progression < 0.75f && Main.rand.NextBool(5))
            {
                Vector2 pollenVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                SakuraParticleHandler.SpawnParticle(new PollenMoteParticle(
                    tipPos + Main.rand.NextVector2Circular(20f, 20f), pollenVel,
                    GoldenPollen, Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(40, 70)));
            }

            // Music notes scatter (Phase 2+)
            if (_comboStep >= 2 && Progression > 0.35f && Progression < 0.75f && Main.rand.NextBool(6))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                Color noteColor = GetSakuraFireGradient(Main.rand.NextFloat());
                SakuraParticleHandler.SpawnParticle(new SakuraNoteParticle(
                    tipPos + Main.rand.NextVector2Circular(20f, 20f), noteVel,
                    noteColor, Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(40, 70)));
            }

            // Sakura bloom pulse at swing peak for Phase 3
            if (_comboStep == 3 && Math.Abs(Progression - 0.55f) < 0.02f)
            {
                SakuraParticleHandler.SpawnParticle(new SakuraBloomParticle(
                    tipPos, Vector2.Zero, SakuraPink, 0.8f, 30));
                SakuraParticleHandler.SpawnParticle(new SakuraBloomParticle(
                    Owner.MountedCenter, Vector2.Zero, GoldenPollen, 1.2f, 40));
            }
        }

        private void SpawnComboProjectiles()
        {
            if (Main.myPlayer != Projectile.owner) return;

            Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
            Vector2 dir = SwordDirection.SafeNormalize(Vector2.UnitX);

            // Phase 0: 1 spectral copy at 60%
            if (_comboStep == 0 && Math.Abs(Timer - SwingTime * 0.60f) < 1f)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), tipPos,
                    dir * 15f,
                    ModContent.ProjectileType<SakurasBlossomSpectral>(),
                    (int)(Projectile.damage * 0.7f), 3f, Projectile.owner);
            }

            // Phase 1: 2 spectral copies with spread at 55%
            if (_comboStep == 1 && Math.Abs(Timer - SwingTime * 0.55f) < 1f)
            {
                float spread = MathHelper.ToRadians(25f);
                for (int i = -1; i <= 1; i += 2)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), tipPos,
                        dir.RotatedBy(spread * i) * 14f,
                        ModContent.ProjectileType<SakurasBlossomSpectral>(),
                        (int)(Projectile.damage * 0.7f), 3f, Projectile.owner);
                }
            }

            // Phase 2: 3 spectral copies in fan at 65%
            if (_comboStep == 2 && Math.Abs(Timer - SwingTime * 0.65f) < 1f)
            {
                float spread = MathHelper.ToRadians(20f);
                for (int i = -1; i <= 1; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), tipPos,
                        dir.RotatedBy(spread * i) * 15f,
                        ModContent.ProjectileType<SakurasBlossomSpectral>(),
                        (int)(Projectile.damage * 0.75f), 3f, Projectile.owner);
                }
            }

            // Phase 3: 4 spectral copies at 55% + finisher sound
            if (_comboStep == 3 && Math.Abs(Timer - SwingTime * 0.55f) < 1f)
            {
                float spread = MathHelper.ToRadians(18f);
                for (int i = 0; i < 4; i++)
                {
                    float angle = spread * (i - 1.5f);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), tipPos,
                        dir.RotatedBy(angle) * 16f,
                        ModContent.ProjectileType<SakurasBlossomSpectral>(),
                        (int)(Projectile.damage * 0.8f), 4f, Projectile.owner);
                }
                SoundEngine.PlaySound(SoundID.Item70 with { Pitch = -0.3f, Volume = 0.5f }, tipPos);
            }
        }

        #endregion

        #region Petal Dash (Special Attack)

        public void DoBehavior_PetalDash()
        {
            Owner.mount?.Dismount(Owner);
            Owner.RemoveAllGrapplingHooks();

            if (DashProgression == 0)
            {
                // Sound cue before dash
                if (Projectile.timeLeft == 1 + (int)(SwingTime * DashPercentage))
                    SoundEngine.PlaySound(SoundID.Item66 with { Volume = 0.7f, Pitch = 0.3f }, Projectile.Center);

                Projectile.velocity = Owner.MountedCenter.DirectionTo(Main.MouseWorld);
                Projectile.oldPos = new Vector2[Projectile.oldPos.Length];
                for (int i = 0; i < Projectile.oldPos.Length; ++i)
                    Projectile.oldPos[i] = Projectile.position;
            }
            else
            {
                // Gentle course correction during dash
                float correctionStrength = MathHelper.PiOver4 * 0.04f * (float)Math.Pow(DashProgression, 3);
                float currentRotation = Projectile.velocity.ToRotation();
                float idealRotation = Owner.MountedCenter.DirectionTo(Main.MouseWorld).ToRotation();
                Projectile.velocity = currentRotation.AngleTowards(idealRotation, correctionStrength).ToRotationVector2();

                Owner.fallStart = (int)(Owner.position.Y / 16f);

                float velocityPower = (float)Math.Sin(MathHelper.Pi * DashProgression);
                velocityPower = (float)Math.Pow(Math.Abs(velocityPower), 0.6f);
                Vector2 newVelocity = Projectile.velocity * DashSpeed * (0.2f + 0.8f * velocityPower);
                Owner.velocity = newVelocity;
                Owner.SakuraBlossom().IsLunging = true;

                // Petal dust scattering during dash
                if (Main.rand.NextBool())
                {
                    Dust d = Dust.NewDustPerfect(Owner.MountedCenter + Main.rand.NextVector2Circular(20f, 20f),
                        ModContent.DustType<SakuraPetalDust>(), SwordDirection * -2.6f);
                    d.scale = 0.5f;
                    d.noGravity = true;
                }

                // Sakura petals trailing the dash
                if (Main.rand.NextBool(3) && DashProgression < 0.85f && !Main.dedServ)
                {
                    Vector2 particleSpeed = SwordDirection * -1 * Main.rand.NextFloat(4f, 8f);
                    SakuraParticleHandler.SpawnParticle(new SakuraPetalParticle(
                        Owner.MountedCenter + Main.rand.NextVector2Circular(20f, 20f) + Owner.velocity * 4,
                        particleSpeed, GetPetalGradient(Main.rand.NextFloat()),
                        Main.rand.NextFloat(0.4f, 0.8f), 40));
                }

                // Blossom sparks trailing the dash
                if (Main.rand.NextBool(5) && !Main.dedServ)
                {
                    Vector2 sparkSpeed = SwordDirection * -1 * Main.rand.NextFloat(6f, 10f);
                    SakuraParticleHandler.SpawnParticle(new BlossomSparkParticle(
                        Owner.MountedCenter + Main.rand.NextVector2Circular(30f, 30f),
                        sparkSpeed, Color.Lerp(SpringWhite, SakuraPink, 0.5f),
                        Main.rand.NextFloat(0.4f, 0.7f), 20));
                }

                // Pollen motes during dash
                if (Main.rand.NextBool(6) && !Main.dedServ)
                {
                    SakuraParticleHandler.SpawnParticle(new PollenMoteParticle(
                        Owner.MountedCenter + Main.rand.NextVector2Circular(25f, 25f),
                        Main.rand.NextVector2Unit() * Main.rand.NextFloat(1f, 3f),
                        GoldenPollen, Main.rand.NextFloat(0.3f, 0.5f), 30));
                }

                // Sakura light along dash path
                Lighting.AddLight(Owner.MountedCenter, SakuraPink.ToVector3() * 0.8f);
            }

            // Stop the dash on last frame
            if (Projectile.timeLeft == 1)
                Owner.velocity *= 0.15f;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 * Direction;
        }

        #endregion

        #region Trail Width/Color Functions

        public float SlashWidthFunction(float completionRatio)
        {
            float squish = SquishAtProgress(RealProgressionAtTrailCompletion(completionRatio));
            return squish * Projectile.scale * 55f * _phaseIntensity;
        }

        public Color SlashColorFunction(float completionRatio)
        {
            float fade = Utils.GetLerpValue(0.9f, 0.35f, completionRatio, true);
            Color baseColor = Color.Lerp(SakuraPink, CrimsonFlame, completionRatio * 0.5f + _phaseIntensity * 0.2f);
            baseColor.A = 0;
            return baseColor * fade * Projectile.Opacity;
        }

        public float GlowWidthFunction(float completionRatio) =>
            SlashWidthFunction(completionRatio) * 1.5f;

        public Color GlowColorFunction(float completionRatio)
        {
            float fade = Utils.GetLerpValue(0.95f, 0.3f, completionRatio, true);
            Color glowColor = Color.Lerp(DeepCrimson, SakuraPink, completionRatio * 0.5f);
            glowColor.A = 0;
            return glowColor * fade * 0.5f * Projectile.Opacity;
        }

        public float DashWidthFunction(float completionRatio)
        {
            float width = Utils.GetLerpValue(0f, 0.2f, completionRatio, true) * Projectile.scale * 45f;
            width *= (1 - (float)Math.Pow(DashProgression, 5));
            return width;
        }

        public Color DashColorFunction(float completionRatio)
        {
            Color c = Color.Lerp(SakuraPink, CrimsonFlame, completionRatio * 0.5f);
            c.A = 0;
            return c * Projectile.Opacity;
        }

        #endregion

        #region Slash Point Generation

        public List<Vector2> GenerateSlashPoints()
        {
            List<Vector2> result = new();
            for (int i = 0; i < 40; i++)
            {
                float progress = MathHelper.Lerp(Progression, TrailEndProgression, i / 40f);
                result.Add(DirectionAtProgressSmoothed(progress) * (BladeLength - 6f) * Projectile.scale);
            }
            return result;
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.Opacity <= 0f || InPostDashStasis)
                return false;

            DrawSakuraGlow();
            DrawSakuraTrail();
            DrawDashTrail();
            DrawBlade();
            DrawPetalBloom();
            return false;
        }

        /// <summary>Layer 1: Wide, soft sakura-pink bloom underlayer via SakuraTrailGlow shader.</summary>
        public void DrawSakuraGlow()
        {
            if (State != SwingState.Swinging || Progression < 0.4f)
                return;

            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);

            var shader = GameShaders.Misc[SakuraShaderLoader.SakuraTrailGlowKey];
            _noiseTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");

            shader.UseImage1(_noiseTexture);
            shader.UseColor(SakuraPink);
            shader.UseSecondaryColor(GoldenPollen);
            shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 1.5f);
            shader.Shader.Parameters["uIntensity"]?.SetValue(_phaseIntensity);
            shader.Shader.Parameters["uOpacity"]?.SetValue(0.55f * _phaseIntensity);
            shader.Shader.Parameters["uOverbrightMult"]?.SetValue(1.8f + _phaseIntensity);
            shader.Apply();

            var localPoints = GenerateSlashPoints();
            Vector2[] worldPositions = new Vector2[localPoints.Count];
            for (int i = 0; i < localPoints.Count; i++)
                worldPositions[i] = Projectile.Center + localPoints[i];

            SakuraTrailRenderer.RenderTrail(worldPositions, new SakuraTrailSettings(
                GlowWidthFunction, GlowColorFunction, smoothen: true, shader: shader), 40);

            Main.spriteBatch.ExitShaderRegion();
        }

        /// <summary>Layer 2: Core sakura trail with petal-to-crimson flame gradient.</summary>
        public void DrawSakuraTrail()
        {
            if (State != SwingState.Swinging || Progression < 0.42f)
                return;

            Main.spriteBatch.EnterShaderRegion();

            var shader = GameShaders.Misc[SakuraShaderLoader.SakuraTrailFlowKey];
            _noiseTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");

            shader.UseImage1(_noiseTexture);
            shader.UseColor(CrimsonFlame);
            shader.UseSecondaryColor(SakuraPink);
            shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
            shader.Shader.Parameters["uIntensity"]?.SetValue(_phaseIntensity);
            shader.Shader.Parameters["uOpacity"]?.SetValue(1.0f);
            shader.Shader.Parameters["uOverbrightMult"]?.SetValue(2.2f + _phaseIntensity * 0.5f);
            shader.Apply();

            var localPoints = GenerateSlashPoints();
            Vector2[] worldPositions = new Vector2[localPoints.Count];
            for (int i = 0; i < localPoints.Count; i++)
                worldPositions[i] = Projectile.Center + localPoints[i];

            SakuraTrailRenderer.RenderTrail(worldPositions, new SakuraTrailSettings(
                SlashWidthFunction, SlashColorFunction, smoothen: true, shader: shader), 40);

            Main.spriteBatch.ExitShaderRegion();
        }

        /// <summary>Layer 3: Petal dash trail using trailing positions.</summary>
        public void DrawDashTrail()
        {
            if (State != SwingState.PetalDash)
                return;

            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);

            var shader = GameShaders.Misc[SakuraShaderLoader.SakuraTrailGlowKey];
            _noiseTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");

            Color mainColor = Color.Lerp(SakuraPink, CrimsonFlame,
                (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);
            Color secondaryColor = Color.Lerp(GoldenPollen, SpringWhite,
                (float)Math.Cos(Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);

            shader.UseImage1(_noiseTexture);
            shader.UseColor(mainColor);
            shader.UseSecondaryColor(secondaryColor);
            shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
            shader.Shader.Parameters["uIntensity"]?.SetValue(0.8f);
            shader.Shader.Parameters["uOpacity"]?.SetValue(0.85f);
            shader.Shader.Parameters["uOverbrightMult"]?.SetValue(2.0f);
            shader.Apply();

            var positionsToUse = Projectile.oldPos.Take(50).ToArray();

            SakuraTrailRenderer.RenderTrail(positionsToUse, new SakuraTrailSettings(
                DashWidthFunction, DashColorFunction, smoothen: true, shader: shader), 25);

            Main.spriteBatch.ExitShaderRegion();
        }

        /// <summary>Layer 4: Blade sprite with rotation + cross flare at tip.</summary>
        public void DrawBlade()
        {
            var texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            SpriteEffects direction = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (State == SwingState.Swinging)
            {
                float rotation = SwordRotation + MathHelper.PiOver4;
                Vector2 origin = new Vector2(0, texture.Height);

                if (Direction == -1)
                {
                    rotation += MathHelper.PiOver2;
                    origin.X = texture.Width;
                }

                Vector2 drawPosition = Owner.MountedCenter - Main.screenPosition;

                Main.EntitySpriteDraw(texture, drawPosition, null,
                    Color.White, rotation, origin, SquishVector * 2.8f * Projectile.scale, direction, 0);

                // Additive energy glow copies in sakura pink-gold
                float energyPower = Utils.GetLerpValue(0.2f, 0.4f, Progression, true) *
                    Utils.GetLerpValue(0.9f, 0.75f, Progression, true) * _phaseIntensity;
                if (energyPower > 0)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                        Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null,
                        Main.GameViewMatrix.TransformationMatrix);

                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 drawOffset = (MathHelper.TwoPi * i / 4f + SwordRotation).ToRotationVector2() *
                            energyPower * Projectile.scale * 6f;
                        Color glowColor = Color.Lerp(SakuraPink, GoldenPollen, i / 3f);
                        glowColor.A = 0;
                        Main.EntitySpriteDraw(texture, drawPosition + drawOffset, null,
                            glowColor * 0.14f, rotation, origin, SquishVector * 2.8f * Projectile.scale, direction, 0);
                    }

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                        Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null,
                        Main.GameViewMatrix.TransformationMatrix);
                }

                // === LENS FLARE AT BLADE TIP ===
                _lensFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar");
                Texture2D shineTex = _lensFlare.Value;
                Vector2 shineScale = new Vector2(1f, 2.5f);

                float flareOpacity = (Progression < 0.25f ? 0f :
                    0.15f + 0.85f * (float)Math.Sin(MathHelper.Pi * (Progression - 0.25f) / 0.75f))
                    * 0.5f * _phaseIntensity;
                Color flareColor = Color.Lerp(SakuraPink, GoldenPollen, (float)Math.Pow(Progression, 2));
                flareColor.A = 0;

                Vector2 tipDrawPos = Owner.MountedCenter + DirectionAtProgressSmoothed(Progression) *
                    Projectile.scale * BladeLength - Main.screenPosition;

                Main.EntitySpriteDraw(shineTex, tipDrawPos, null,
                    flareColor * flareOpacity, MathHelper.PiOver2,
                    shineTex.Size() / 2f, shineScale * Projectile.scale, 0, 0);

                // Cross-star
                Main.EntitySpriteDraw(shineTex, tipDrawPos, null,
                    flareColor * flareOpacity * 0.6f, 0f,
                    shineTex.Size() / 2f, shineScale * Projectile.scale * 0.7f, 0, 0);
            }
            else
            {
                // During Petal Dash: standard sprite draw with energy glow copies
                float rotation = BaseRotation + MathHelper.PiOver4;
                Vector2 origin = new Vector2(0, texture.Height);
                Vector2 drawPosition = Projectile.Center + Projectile.velocity * Projectile.scale * DashDisplace - Main.screenPosition;

                if (Direction == -1)
                {
                    rotation += MathHelper.PiOver2;
                    origin.X = texture.Width;
                }

                Projectile.scale = MathHelper.Lerp(1f, 0.25f, MathF.Pow(DashProgression, 6));

                Main.EntitySpriteDraw(texture, drawPosition, null, Color.White, rotation, origin, Projectile.scale, direction, 0);

                // Additive sakura energy glow
                float energyPower = Utils.GetLerpValue(0f, 0.3f, Progression, true) *
                    Utils.GetLerpValue(1f, 0.85f, Progression, true);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 4f + BaseRotation).ToRotationVector2() * energyPower * Projectile.scale * 6f;
                    Color glowColor = Color.Lerp(SakuraPink, CrimsonFlame, Progression);
                    glowColor.A = 0;
                    Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null,
                        glowColor * 0.14f, rotation, origin, Projectile.scale, direction, 0);
                }
            }
        }

        /// <summary>Layer 5: Soft petal bloom overlay at blade tip (Phase 1+).</summary>
        public void DrawPetalBloom()
        {
            if (State != SwingState.Swinging || _comboStep < 1 || Progression < 0.3f || Progression > 0.85f)
                return;

            _bloomCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            Texture2D bloom = _bloomCircle.Value;

            Vector2 tipPos = Owner.MountedCenter + DirectionAtProgressSmoothed(Progression) *
                Projectile.scale * BladeLength - Main.screenPosition;

            float bloomScale = 0.35f + 0.3f * _phaseIntensity;
            float bloomOpacity = (float)Math.Sin(MathHelper.Pi * (Progression - 0.3f) / 0.55f) * 0.5f * _phaseIntensity;

            // Inner: bright sakura pink
            Color innerColor = SakuraPink;
            innerColor.A = 0;
            Main.spriteBatch.Draw(bloom, tipPos, null, innerColor * bloomOpacity,
                SwordRotation, bloom.Size() / 2f, bloomScale * 0.5f * Projectile.scale, SpriteEffects.None, 0f);

            // Outer: soft golden pollen
            Color outerColor = GoldenPollen;
            outerColor.A = 0;
            Main.spriteBatch.Draw(bloom, tipPos, null, outerColor * bloomOpacity * 0.4f,
                SwordRotation, bloom.Size() / 2f, bloomScale * Projectile.scale, SpriteEffects.None, 0f);
        }

        #endregion

        #region On Hit

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ItemLoader.OnHitNPC(Owner.HeldItem, Owner, target, hit, damageDone);
            NPCLoader.OnHitByItem(target, Owner, Owner.HeldItem, hit, damageDone);
            PlayerLoader.OnHitNPC(Owner, target, hit, damageDone);

            // Apply MusicsDissonance + SakuraBlight debuffs
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);
            target.AddBuff(ModContent.BuffType<SakuraBlight>(), 180);

            // === PETAL DASH HIT ===
            if (State == SwingState.PetalDash)
            {
                Owner.itemAnimation = 0;
                Owner.velocity = Owner.SafeDirectionTo(target.Center) * -ReboundSpeed;
                Projectile.timeLeft = EmpoweredOpportunity + DashCooldown;
                InPostDashStasis = true;
                Projectile.netUpdate = true;

                SoundEngine.PlaySound(SoundID.Item125 with { Volume = 0.8f }, target.Center);

                // Apply PetalWound
                target.AddBuff(ModContent.BuffType<PetalWound>(), 120);

                // Sakura petal explosion at impact
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 petalVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 8f);
                        SakuraParticleHandler.SpawnParticle(new SakuraPetalParticle(
                            target.Center + Main.rand.NextVector2Circular(15f, 15f), petalVel,
                            GetPetalGradient(Main.rand.NextFloat()),
                            Main.rand.NextFloat(0.5f, 1f), Main.rand.Next(40, 70)));
                    }

                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 sparkVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 12f);
                        SakuraParticleHandler.SpawnParticle(new BlossomSparkParticle(
                            target.Center, sparkVel,
                            Color.Lerp(SpringWhite, SakuraPink, Main.rand.NextFloat()),
                            Main.rand.NextFloat(0.5f, 1f), 20));
                    }

                    SakuraParticleHandler.SpawnParticle(new SakuraBloomParticle(
                        target.Center, Vector2.Zero, SakuraPink, 1f, 25));
                }
            }

            // === EMPOWERED STORM OF PETALS HIT ===
            if (State == SwingState.Swinging && PerformingEmpoweredFinale)
            {
                SoundEngine.PlaySound(SoundID.Item70 with { Pitch = -0.2f }, Projectile.Center);

                // Lifesteal on empowered hit
                Owner.DoLifestealDirect(target, (int)Math.Round(hit.Damage * 0.05));

                // Massive sakura impact VFX
                if (!Main.dedServ)
                {
                    // Triple bloom burst (pink → gold → white)
                    SakuraParticleHandler.SpawnParticle(new SakuraBloomParticle(
                        target.Center, Vector2.Zero, SakuraPink, 1.5f, 35));
                    SakuraParticleHandler.SpawnParticle(new SakuraBloomParticle(
                        target.Center, Vector2.Zero, GoldenPollen, 2f, 40));
                    SakuraParticleHandler.SpawnParticle(new SakuraBloomParticle(
                        target.Center, Vector2.Zero, SpringWhite, 2.5f, 50));

                    // Blossom spark explosion
                    for (int i = 0; i < 14; i++)
                    {
                        Vector2 sparkVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(6f, 14f);
                        SakuraParticleHandler.SpawnParticle(new BlossomSparkParticle(
                            target.Center, sparkVel,
                            Color.Lerp(SpringWhite, SakuraPink, Main.rand.NextFloat()),
                            Main.rand.NextFloat(0.6f, 1.2f),
                            Main.rand.Next(18, 30)));
                    }

                    // Sakura petal cascade
                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 petalVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 6f)
                            + new Vector2(0, Main.rand.NextFloat(-1f, 1f));
                        SakuraParticleHandler.SpawnParticle(new SakuraPetalParticle(
                            target.Center + Main.rand.NextVector2Circular(30f, 30f),
                            petalVel, GetPetalGradient(Main.rand.NextFloat()),
                            Main.rand.NextFloat(0.5f, 1f), Main.rand.Next(50, 90)));
                    }

                    // Music note cascade
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 noteVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f));
                        SakuraParticleHandler.SpawnParticle(new SakuraNoteParticle(
                            target.Center + Main.rand.NextVector2Circular(30f, 30f),
                            noteVel, GetSakuraFireGradient(Main.rand.NextFloat()),
                            Main.rand.NextFloat(0.4f, 0.8f), Main.rand.Next(50, 80)));
                    }

                    // Pollen mote burst
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 pollenVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1f, 3f) +
                            new Vector2(0, -Main.rand.NextFloat(0.5f, 1.5f));
                        SakuraParticleHandler.SpawnParticle(new PollenMoteParticle(
                            target.Center + Main.rand.NextVector2Circular(40f, 40f),
                            pollenVel, GoldenPollen,
                            Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(60, 100)));
                    }
                }
            }

            // === SEEKING CRYSTALS ON CRIT (all phases) ===
            if (hit.Crit && Main.myPlayer == Projectile.owner)
            {
                SeekingCrystalHelper.SpawnEroicaCrystals(
                    Projectile.GetSource_FromAI(),
                    target.Center,
                    (Main.MouseWorld - target.Center).SafeNormalize(Vector2.UnitX) * 8f,
                    (int)(Projectile.damage * 0.2f),
                    Projectile.knockBack * 0.4f,
                    Projectile.owner,
                    2 + _comboStep * 2); // 2/4/6/8 crystals by phase
            }
        }

        #endregion

        #region On Kill

        public override void OnKill(int timeLeft)
        {
            Owner.fullRotation = 0f;
            Owner.SakuraBlossom().IsLunging = false;
        }

        #endregion
    }
}
