using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Buffs;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Dusts;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Particles;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Primitives;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Shaders;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Utilities;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles;
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
using static MagnumOpus.Content.Eroica.Weapons.CelestialValor.Utilities.ValorUtils;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    /// <summary>
    /// CelestialValorSwing — "The Hero's Burning Oath"
    /// 
    /// The central rendering and combat file for the Celestial Valor melee weapon.
    /// Handles all swing animation, collision, trail rendering, blade drawing, particle spawning,
    /// combo phase tracking, special mechanics, and sub-projectile creation.
    /// 
    /// --- COMBO SYSTEM: HEROIC CRESCENDO ---
    /// 3-phase combo that escalates from whisper to war cry:
    ///   Phase 0: Valor's Whisper    — Swift controlled slash, 1 energy projectile
    ///   Phase 1: Crimson Declaration — Powerful reverse backhand, 2 spread projectiles
    ///   Phase 2: Heroic Finale      — Massive overhead slam, 3 fan projectiles + finisher VFX
    /// 
    /// --- SPECIAL ATTACKS ---
    ///   Alt-click: Valor Dash — Charge forward in blazing glory trailing heroic fire.
    ///                          On hit: applies Stagger, spawns cross-slash VFX
    ///                          If dash connects, next swing is empowered Heroic Finale
    ///   
    ///   Empowered Finale — 1.4x scale, spawns ValorBoom explosion on hit + lifesteal
    ///   
    ///   Phase 2 Seeking Crystals — On crit during Heroic Finale, spawns homing valor crystals
    ///
    /// --- VISUAL LAYERS (render order) ---
    ///   1. Heroic Glow Pass    — Wide, soft scarlet-gold bloom underlayer via ValorFlare shader
    ///   2. Heroic Trail Pass   — Core arc trail using HeroicTrail shader with flame gradient
    ///   3. Dash Trail          — During dash: trailing positions with ValorFlare shader
    ///   4. Blade Sprite        — UV-rotated blade + lens flare crosses at tip
    ///   5. Ember Bloom         — Soft bloom overlay at blade tip (Phase 1+)
    /// </summary>
    public sealed class CelestialValorSwing : ModProjectile
    {
        #region Constants and Properties

        public Player Owner => Main.player[Projectile.owner];

        private const float BladeLength = 162f;
        private const int BaseSwingTime = 72;
        private const float MaxSwingAngle = MathHelper.PiOver2 * 1.8f;
        private const float DashSpeed = 45f;
        private const float DashPercentage = 0.55f;
        private const float EmpoweredUpscale = 1.4f;
        private const float ReboundSpeed = 5f;
        private const int DashCooldown = 60 * 3;
        private const int EmpoweredOpportunity = 37 * 3;
        private const float SubProjectileDamagePenalty = 0.3f;
        private const float FinisherDamageFactor = 1.8f;

        public int GetSwingTime
        {
            get
            {
                if (State == SwingState.ValorDash)
                    return CelestialValor.ValorDashTime * Projectile.extraUpdates;
                return BaseSwingTime;
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
            ValorDash
        }

        public SwingState State
        {
            get => Projectile.ai[0] == 1 ? SwingState.ValorDash : SwingState.Swinging;
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

        /// <summary>Current combo step for this swing (0-2, cached from player on init).</summary>
        private int _comboStep;

        /// <summary>Phase intensity 0.4-1.0 driving shader uniforms and VFX density.</summary>
        private float _phaseIntensity;

        public float IdealSize => PerformingEmpoweredFinale ? EmpoweredUpscale : 1f;

        #endregion

        #region Swing Animation Curves

        public int Direction => Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;
        public float BaseRotation => Projectile.velocity.ToRotation();
        public Vector2 SquishVector => new Vector2(1f + (1 - SquishFactor) * 0.5f, SquishFactor);

        // Phase 0: Valor's Whisper — quick windup, sharp swing, soft follow-through
        public CurveSegment Phase0_Windup = new(PolyOutEasing, 0f, -0.85f, 0.18f, 2);
        public CurveSegment Phase0_Swing = new(PolyInEasing, 0.18f, -0.67f, 1.52f, 3);
        public CurveSegment Phase0_Settle = new(PolyOutEasing, 0.80f, 0.85f, 0.12f, 2);

        // Phase 1: Crimson Declaration — dramatic pullback, explosive swing, weighted decel
        public CurveSegment Phase1_Windup = new(PolyOutEasing, 0f, -1.05f, 0.25f, 2);
        public CurveSegment Phase1_Swing = new(PolyInEasing, 0.25f, -0.80f, 1.85f, 3);
        public CurveSegment Phase1_Settle = new(PolyOutEasing, 0.84f, 1.05f, 0.08f, 2);

        // Phase 2: Heroic Finale — dramatic raise, violent slam, abrupt stop
        public CurveSegment Phase2_Windup = new(SineOutEasing, 0f, -1.15f, 0.16f, 2);
        public CurveSegment Phase2_Swing = new(PolyInEasing, 0.16f, -0.99f, 2.24f, 4);
        public CurveSegment Phase2_Settle = new(PolyOutEasing, 0.78f, 1.25f, 0.03f, 2);

        public float SwingAngleShiftAtProgress(float progress)
        {
            if (State == SwingState.ValorDash) return 0;

            float p;
            switch (_comboStep)
            {
                case 1:
                    p = PiecewiseAnimation(progress, Phase1_Windup, Phase1_Swing, Phase1_Settle);
                    return MaxSwingAngle * 1.05f * p;
                case 2:
                    p = PiecewiseAnimation(progress, Phase2_Windup, Phase2_Swing, Phase2_Settle);
                    return MaxSwingAngle * 1.27f * p;
                default:
                    p = PiecewiseAnimation(progress, Phase0_Windup, Phase0_Swing, Phase0_Settle);
                    return MaxSwingAngle * 0.83f * p;
            }
        }

        public float SwordRotationAtProgress(float progress) =>
            State == SwingState.ValorDash ? BaseRotation :
            BaseRotation + SwingAngleShiftAtProgress(progress) * Direction * (_comboStep == 1 ? -1 : 1);

        public float SquishAtProgress(float progress) =>
            State == SwingState.ValorDash ? 1 :
            MathHelper.Lerp(SquishVector.X, SquishVector.Y,
                (float)Math.Abs(Math.Sin(SwingAngleShiftAtProgress(progress))));

        public Vector2 DirectionAtProgress(float progress) =>
            State == SwingState.ValorDash ? Projectile.velocity :
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

        /// <summary>Risk factor controlling dust emission density during swing.</summary>
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

        public override string Texture => "MagnumOpus/Content/Eroica/Weapons/CelestialValor/CelestialValor";
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

        public override bool ShouldUpdatePosition() => State == SwingState.ValorDash && !InPostDashStasis;

        public override bool? CanDamage()
        {
            if (State != SwingState.ValorDash) return null;
            if (InPostDashStasis) return false;
            if (Projectile.timeLeft > SwingTime * DashPercentage) return false;
            return null;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Projectile.Center;
            Vector2 end = start + SwordDirection * (BladeLength + 40) * Projectile.scale;
            float width = State == SwingState.ValorDash ? Projectile.scale * 42f : Projectile.scale * 28f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref _);
        }

        #endregion

        #region Initialization

        public void InitializationEffects(bool startInit)
        {
            Projectile.velocity = Owner.MountedCenter.DirectionTo(Main.MouseWorld);
            SquishFactor = Main.rand.NextFloat(0.7f, 1f);

            // Cache combo step from player
            _comboStep = Owner.CelestialValor().ComboStep;
            _phaseIntensity = 0.4f + _comboStep * 0.3f; // 0.4, 0.7, 1.0

            if (startInit && State != SwingState.ValorDash)
                Projectile.scale = 0.02f;
            else
            {
                Projectile.scale = 1f;
                if (PerformingEmpoweredFinale)
                {
                    State = SwingState.Swinging;
                    _comboStep = 2;
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

            if (Projectile.timeLeft >= 9999 || (Projectile.timeLeft == 1 && Owner.channel && State != SwingState.ValorDash))
                InitializationEffects(Projectile.timeLeft >= 9999);

            switch (State)
            {
                case SwingState.Swinging:
                    DoBehavior_Swinging();
                    break;
                case SwingState.ValorDash:
                    DoBehavior_ValorDash();
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
            if (Projectile.timeLeft == 1 && State == SwingState.ValorDash && !InPostDashStasis)
            {
                Projectile.timeLeft = DashCooldown;
                InPostDashStasis = true;
                Owner.fullRotation = 0f;
                Owner.CelestialValor().IsLunging = false;
            }
        }

        #endregion

        #region Swing Behavior

        public void DoBehavior_Swinging()
        {
            // Play swing sound at 20% through with escalating pitch
            if (Projectile.timeLeft == (int)(SwingTime / 5))
            {
                SoundEngine.PlaySound(SoundID.Item71 with
                {
                    Volume = 0.85f + _comboStep * 0.08f,
                    Pitch = -0.25f + _comboStep * 0.22f,
                    PitchVariance = 0.25f
                }, Projectile.Center);
                if (PerformingEmpoweredFinale)
                    SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.5f, Pitch = -0.4f }, Projectile.Center);
            }

            // Dynamic heroic lighting along the blade
            Vector3 lightColor = Color.Lerp(ScarletEmber, GoldenFlare, (float)Math.Pow(Progression, 2)).ToVector3();
            lightColor *= 1.2f * _phaseIntensity * (float)Math.Sin(Progression * MathHelper.Pi);
            Lighting.AddLight(Owner.MountedCenter + SwordDirection * 90, lightColor);

            // Scale up to ideal size
            if (Projectile.scale < IdealSize)
                Projectile.scale = MathHelper.Lerp(Projectile.scale, IdealSize, 0.08f);

            // Shrink near end of slash
            if (!Owner.channel && Progression > 0.7f)
                Projectile.scale = (0.5f + 0.5f * (float)Math.Pow(1 - (Progression - 0.7f) / 0.3f, 0.5)) * IdealSize;

            // === DUST SPAWNING ===
            // Heroic ember dust from blade edge
            if (Main.rand.NextFloat() * 3f < DustRisk)
            {
                Vector2 dustPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale *
                    (float)Math.Pow(Main.rand.NextFloat(0.5f, 1f), 0.5f);
                Dust d = Dust.NewDustPerfect(dustPos, ModContent.DustType<HeroicEmberDust>(),
                    SwordDirection.RotatedBy(-MathHelper.PiOver2 * Direction) * 2f);
                d.noGravity = true;
                d.alpha = 10;
                d.scale = 0.5f * _phaseIntensity;
            }

            // Scarlet/gold ambient dust
            if (Main.rand.NextFloat() < DustRisk * 0.7f)
            {
                Color dustColor = Color.Lerp(ScarletEmber, GoldenFlare, Main.rand.NextFloat());
                Vector2 dustPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale *
                    (float)Math.Pow(Main.rand.NextFloat(0.2f, 1f), 0.5f);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.GoldFlame,
                    SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction) * 2.2f, 0, dustColor);
                d.scale = 0.4f;
                d.fadeIn = Main.rand.NextFloat() * 1.0f;
                d.noGravity = true;
            }

            // === PARTICLE SPAWNING ===
            SpawnSwingParticles();

            // === ENERGY PROJECTILES ===
            SpawnComboProjectiles();

            // Advance combo on first swing frame
            if (Timer == 1)
                Owner.CelestialValor().AdvanceCombo();
        }

        private void SpawnSwingParticles()
        {
            if (Main.dedServ) return;

            float tipX = BladeLength * Projectile.scale;
            Vector2 tipPos = Owner.MountedCenter + SwordDirection * tipX;

            // Heroic embers along blade — density scales with combo step
            if (Main.rand.NextFloat() < 0.3f * _phaseIntensity && Progression > 0.2f && Progression < 0.85f)
            {
                Vector2 moteVel = SwordDirection.RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 3f);
                Color moteColor = Color.Lerp(ScarletEmber, GoldenFlare, Main.rand.NextFloat());
                ValorParticleHandler.SpawnParticle(new HeroicEmberParticle(
                    tipPos + Main.rand.NextVector2Circular(15f, 15f), moteVel,
                    moteColor, Main.rand.NextFloat(0.3f, 0.7f) * _phaseIntensity,
                    Main.rand.Next(25, 50)));
            }

            // Valor sparks on fast part of swing (Phase 1+)
            if (_comboStep >= 1 && Progression > 0.4f && Progression < 0.8f && Main.rand.NextBool(4))
            {
                Vector2 sparkVel = SwordDirection.RotatedByRandom(0.3f) * Main.rand.NextFloat(4f, 8f);
                ValorParticleHandler.SpawnParticle(new ValorSparkParticle(
                    tipPos, sparkVel, Color.Lerp(Color.White, GoldenFlare, 0.3f),
                    Main.rand.NextFloat(0.4f, 0.8f), Main.rand.Next(15, 25)));
            }

            // Music notes scatter (Phase 2 / Heroic Finale)
            if (_comboStep >= 2 && Progression > 0.35f && Progression < 0.75f && Main.rand.NextBool(6))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                Color noteColor = GetHeroicGradient(Main.rand.NextFloat());
                ValorParticleHandler.SpawnParticle(new SakuraNoteParticle(
                    tipPos + Main.rand.NextVector2Circular(20f, 20f), noteVel,
                    noteColor, Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(40, 70)));
            }

            // Heroic bloom pulse at swing peak
            if (_comboStep == 2 && Math.Abs(Progression - 0.55f) < 0.02f)
            {
                ValorParticleHandler.SpawnParticle(new HeroicBloomParticle(
                    tipPos, Vector2.Zero, GoldenFlare, 0.8f, 30));
                ValorParticleHandler.SpawnParticle(new HeroicBloomParticle(
                    Owner.MountedCenter, Vector2.Zero, ScarletEmber, 1.2f, 40));
            }
        }

        private void SpawnComboProjectiles()
        {
            if (Main.myPlayer != Projectile.owner) return;

            // Phase 0: 1 projectile at 55%
            if (_comboStep == 0 && Math.Abs(Timer - SwingTime * 0.55f) < 1f)
            {
                Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), tipPos,
                    SwordDirection.SafeNormalize(Vector2.UnitX) * 14f,
                    ModContent.ProjectileType<CelestialValorProjectile>(),
                    (int)(Projectile.damage * 0.88f), 3f, Projectile.owner);
            }

            // Phase 1: 2 projectiles with spread at 50%
            if (_comboStep == 1 && Math.Abs(Timer - SwingTime * 0.50f) < 1f)
            {
                Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Vector2 dir = SwordDirection.SafeNormalize(Vector2.UnitX);
                float spread = MathHelper.ToRadians(9f);
                for (int i = -1; i <= 1; i += 2)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), tipPos,
                        dir.RotatedBy(spread * i) * 14.5f,
                        ModContent.ProjectileType<CelestialValorProjectile>(),
                        (int)(Projectile.damage * 0.88f), 3f, Projectile.owner);
                }
            }

            // Phase 2: 3 projectiles in fan at 58%
            if (_comboStep == 2 && Math.Abs(Timer - SwingTime * 0.58f) < 1f)
            {
                Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Vector2 dir = SwordDirection.SafeNormalize(Vector2.UnitX);
                float spread = MathHelper.ToRadians(13f);
                for (int i = -1; i <= 1; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), tipPos,
                        dir.RotatedBy(spread * i) * 15.5f,
                        ModContent.ProjectileType<CelestialValorProjectile>(),
                        (int)(Projectile.damage * 0.95f), 4.5f, Projectile.owner);
                }
                SoundEngine.PlaySound(SoundID.Item70 with { Pitch = -0.4f, Volume = 0.5f }, tipPos);
            }
        }

        #endregion

        #region Valor Dash (Special Attack) Behavior

        public void DoBehavior_ValorDash()
        {
            Owner.mount?.Dismount(Owner);
            Owner.RemoveAllGrapplingHooks();

            if (DashProgression == 0)
            {
                // Sound cue before dash
                if (Projectile.timeLeft == 1 + (int)(SwingTime * DashPercentage))
                    SoundEngine.PlaySound(SoundID.Item66 with { Volume = 0.7f, Pitch = 0.2f }, Projectile.Center);

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
                Owner.CelestialValor().IsLunging = true;

                // Heroic ember dust during dash
                if (Main.rand.NextBool())
                {
                    Dust d = Dust.NewDustPerfect(Owner.MountedCenter + Main.rand.NextVector2Circular(20f, 20f),
                        ModContent.DustType<HeroicEmberDust>(), SwordDirection * -2.6f);
                    d.scale = 0.5f;
                    d.noGravity = true;
                }

                // Heroic ember particles during dash
                if (Main.rand.NextBool(4) && DashProgression < 0.85f && !Main.dedServ)
                {
                    Vector2 particleSpeed = SwordDirection * -1 * Main.rand.NextFloat(5f, 9f);
                    ValorParticleHandler.SpawnParticle(new HeroicEmberParticle(
                        Owner.MountedCenter + Main.rand.NextVector2Circular(20f, 20f) + Owner.velocity * 4,
                        particleSpeed, Color.Lerp(ScarletEmber, GoldenFlare, Main.rand.NextFloat()),
                        Main.rand.NextFloat(0.3f, 0.6f), 30));
                }

                // Valor sparks trailing the dash
                if (Main.rand.NextBool(5) && !Main.dedServ)
                {
                    Vector2 sparkSpeed = SwordDirection * -1 * Main.rand.NextFloat(6f, 10f);
                    ValorParticleHandler.SpawnParticle(new ValorSparkParticle(
                        Owner.MountedCenter + Main.rand.NextVector2Circular(30f, 30f),
                        sparkSpeed, Color.Lerp(Color.White, GoldenFlare, 0.5f),
                        Main.rand.NextFloat(0.4f, 0.7f), 20));
                }

                // Heroic light along dash path
                Lighting.AddLight(Owner.MountedCenter, GoldenFlare.ToVector3() * 0.8f);
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
            Color baseColor = Color.Lerp(ScarletEmber, GoldenFlare, completionRatio * 0.6f + _phaseIntensity * 0.2f);
            baseColor.A = 0; // Additive-ready
            return baseColor * fade * Projectile.Opacity;
        }

        public float GlowWidthFunction(float completionRatio) =>
            SlashWidthFunction(completionRatio) * 1.5f;

        public Color GlowColorFunction(float completionRatio)
        {
            float fade = Utils.GetLerpValue(0.95f, 0.3f, completionRatio, true);
            Color glowColor = Color.Lerp(BlackSmoke, ScarletEmber, completionRatio * 0.5f);
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
            Color c = Color.Lerp(GoldenFlare, ScarletEmber, completionRatio * 0.5f);
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

            DrawHeroicGlow();
            DrawHeroicTrail();
            DrawDashTrail();
            DrawBlade();
            DrawEmberBloom();
            return false;
        }

        /// <summary>Layer 1: Wide, soft scarlet-gold bloom underlayer via ValorFlare shader.</summary>
        public void DrawHeroicGlow()
        {
            if (State != SwingState.Swinging || Progression < 0.4f)
                return;

            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);

            var shader = GameShaders.Misc[ValorShaderLoader.ValorFlareKey];
            _noiseTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");

            shader.UseImage1(_noiseTexture);
            shader.UseColor(ScarletEmber);
            shader.UseSecondaryColor(GoldenFlare);
            shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 1.5f);
            shader.Shader.Parameters["uIntensity"]?.SetValue(_phaseIntensity);
            shader.Shader.Parameters["uOpacity"]?.SetValue(0.6f * _phaseIntensity);
            shader.Shader.Parameters["uOverbrightMult"]?.SetValue(2.0f + _phaseIntensity);
            shader.Apply();

            // Generate trail positions offset to world space for the renderer
            var localPoints = GenerateSlashPoints();
            Vector2[] worldPositions = new Vector2[localPoints.Count];
            for (int i = 0; i < localPoints.Count; i++)
                worldPositions[i] = Projectile.Center + localPoints[i];

            ValorTrailRenderer.RenderTrail(worldPositions, new ValorTrailSettings(
                GlowWidthFunction, GlowColorFunction, smoothen: true, shader: shader), 40);

            Main.spriteBatch.ExitShaderRegion();
        }

        /// <summary>Layer 2: Core heroic trail with scarlet-gold flame gradient.</summary>
        public void DrawHeroicTrail()
        {
            if (State != SwingState.Swinging || Progression < 0.42f)
                return;

            Main.spriteBatch.EnterShaderRegion();

            var shader = GameShaders.Misc[ValorShaderLoader.HeroicTrailKey];
            _noiseTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");

            shader.UseImage1(_noiseTexture);
            shader.UseColor(ScarletEmber);
            shader.UseSecondaryColor(GoldenFlare);
            shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
            shader.Shader.Parameters["uIntensity"]?.SetValue(_phaseIntensity);
            shader.Shader.Parameters["uOpacity"]?.SetValue(1.0f);
            shader.Shader.Parameters["uOverbrightMult"]?.SetValue(2.5f + _phaseIntensity * 0.5f);
            shader.Apply();

            var localPoints = GenerateSlashPoints();
            Vector2[] worldPositions = new Vector2[localPoints.Count];
            for (int i = 0; i < localPoints.Count; i++)
                worldPositions[i] = Projectile.Center + localPoints[i];

            ValorTrailRenderer.RenderTrail(worldPositions, new ValorTrailSettings(
                SlashWidthFunction, SlashColorFunction, smoothen: true, shader: shader), 40);

            Main.spriteBatch.ExitShaderRegion();
        }

        /// <summary>Layer 3: Dash trail using trailing positions.</summary>
        public void DrawDashTrail()
        {
            if (State != SwingState.ValorDash)
                return;

            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);

            var shader = GameShaders.Misc[ValorShaderLoader.ValorFlareKey];
            _noiseTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");

            Color mainColor = Color.Lerp(GoldenFlare, ScarletEmber,
                (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);
            Color secondaryColor = Color.Lerp(ScarletEmber, WhiteFlash,
                (float)Math.Cos(Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);

            shader.UseImage1(_noiseTexture);
            shader.UseColor(mainColor);
            shader.UseSecondaryColor(secondaryColor);
            shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
            shader.Shader.Parameters["uIntensity"]?.SetValue(0.8f);
            shader.Shader.Parameters["uOpacity"]?.SetValue(0.9f);
            shader.Shader.Parameters["uOverbrightMult"]?.SetValue(2.5f);
            shader.Apply();

            var positionsToUse = Projectile.oldPos.Take(50).ToArray();

            ValorTrailRenderer.RenderTrail(positionsToUse, new ValorTrailSettings(
                DashWidthFunction, DashColorFunction, smoothen: true, shader: shader), 25);

            Main.spriteBatch.ExitShaderRegion();
        }

        /// <summary>Layer 4: Blade sprite with rotation + lens flare crosses at tip.</summary>
        public void DrawBlade()
        {
            var texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            SpriteEffects direction = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (State == SwingState.Swinging)
            {
                // Draw blade sprite directly with rotation
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

                // Additive energy glow copies in heroic scarlet-gold
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
                        Color glowColor = Color.Lerp(ScarletEmber, GoldenFlare, i / 3f);
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
                Color flareColor = Color.Lerp(ScarletEmber, GoldenFlare, (float)Math.Pow(Progression, 2));
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
                // During Valor Dash: standard sprite draw with energy glow copies
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

                // Additive energy glow (golden heroic fire)
                float energyPower = Utils.GetLerpValue(0f, 0.3f, Progression, true) *
                    Utils.GetLerpValue(1f, 0.85f, Progression, true);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 4f + BaseRotation).ToRotationVector2() * energyPower * Projectile.scale * 6f;
                    Color glowColor = Color.Lerp(GoldenFlare, ScarletEmber, Progression);
                    glowColor.A = 0;
                    Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null,
                        glowColor * 0.14f, rotation, origin, Projectile.scale, direction, 0);
                }
            }
        }

        /// <summary>Layer 5: Soft ember bloom overlay at blade tip (Phase 1+).</summary>
        public void DrawEmberBloom()
        {
            if (State != SwingState.Swinging || _comboStep < 1 || Progression < 0.3f || Progression > 0.85f)
                return;

            _bloomCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            Texture2D bloom = _bloomCircle.Value;

            Vector2 tipPos = Owner.MountedCenter + DirectionAtProgressSmoothed(Progression) *
                Projectile.scale * BladeLength - Main.screenPosition;

            float bloomScale = 0.4f + 0.3f * _phaseIntensity;
            float bloomOpacity = (float)Math.Sin(MathHelper.Pi * (Progression - 0.3f) / 0.55f) * 0.5f * _phaseIntensity;

            // Inner: bright golden flare
            Color innerColor = GoldenFlare;
            innerColor.A = 0;
            Main.spriteBatch.Draw(bloom, tipPos, null, innerColor * bloomOpacity,
                SwordRotation, bloom.Size() / 2f, bloomScale * 0.5f * Projectile.scale, SpriteEffects.None, 0f);

            // Outer: soft scarlet
            Color outerColor = ScarletEmber;
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

            // Apply MusicsDissonance + HeroicBurn debuffs
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);
            target.AddBuff(ModContent.BuffType<HeroicBurn>(), 180);

            // === VALOR DASH HIT ===
            if (State == SwingState.ValorDash)
            {
                Owner.itemAnimation = 0;
                Owner.velocity = Owner.SafeDirectionTo(target.Center) * -ReboundSpeed;
                Projectile.timeLeft = EmpoweredOpportunity + DashCooldown;
                InPostDashStasis = true;
                Projectile.netUpdate = true;

                SoundEngine.PlaySound(SoundID.Item125 with { Volume = 0.8f }, target.Center);

                // Apply stagger
                target.AddBuff(ModContent.BuffType<ValorStagger>(), 90);

                // Spawn cross-slash VFX at target
                if (Main.myPlayer == Projectile.owner)
                {
                    int slashDamage = (int)(Projectile.damage * 0.6f);
                    for (int i = 0; i < 3; i++)
                    {
                        int proj = Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                            target.Center, Projectile.velocity * 0.05f,
                            ModContent.ProjectileType<ValorSlashCreator>(),
                            slashDamage, 0f, Projectile.owner, target.whoAmI);
                        if (Main.projectile.IndexInRange(proj))
                            Main.projectile[proj].timeLeft -= i * 5;
                    }
                }

                // Impact particles
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 sparkVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 12f);
                        ValorParticleHandler.SpawnParticle(new ValorSparkParticle(
                            target.Center, sparkVel,
                            Color.Lerp(Color.White, GoldenFlare, Main.rand.NextFloat()),
                            Main.rand.NextFloat(0.5f, 1f), 20));
                    }

                    ValorParticleHandler.SpawnParticle(new HeroicBloomParticle(
                        target.Center, Vector2.Zero, GoldenFlare, 1f, 25));
                }
            }

            // === EMPOWERED HEROIC FINALE HIT ===
            if (State == SwingState.Swinging && PerformingEmpoweredFinale &&
                Owner.ownedProjectileCounts[ModContent.ProjectileType<ValorBoom>()] < 1)
            {
                SoundEngine.PlaySound(SoundID.Item70 with { Pitch = -0.2f }, Projectile.Center);

                if (Main.myPlayer == Projectile.owner)
                {
                    int boomDamage = (int)(Projectile.damage * FinisherDamageFactor);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                        target.Center, Vector2.Zero,
                        ModContent.ProjectileType<ValorBoom>(),
                        boomDamage, 0f, Projectile.owner);
                }

                // Lifesteal on empowered hit
                Owner.DoLifestealDirect(target, (int)Math.Round(hit.Damage * 0.05));

                // Massive impact VFX
                if (!Main.dedServ)
                {
                    // Triple bloom burst (gold to scarlet to black)
                    ValorParticleHandler.SpawnParticle(new HeroicBloomParticle(
                        target.Center, Vector2.Zero, WhiteFlash, 1.5f, 35));
                    ValorParticleHandler.SpawnParticle(new HeroicBloomParticle(
                        target.Center, Vector2.Zero, GoldenFlare, 2f, 40));
                    ValorParticleHandler.SpawnParticle(new HeroicBloomParticle(
                        target.Center, Vector2.Zero, ScarletEmber, 2.5f, 50));

                    // Valor spark explosion
                    for (int i = 0; i < 16; i++)
                    {
                        Vector2 sparkVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(6f, 14f);
                        ValorParticleHandler.SpawnParticle(new ValorSparkParticle(
                            target.Center, sparkVel,
                            Color.Lerp(Color.White, GoldenFlare, Main.rand.NextFloat()),
                            Main.rand.NextFloat(0.6f, 1.2f),
                            Main.rand.Next(18, 30)));
                    }

                    // Music note cascade
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 noteVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f));
                        ValorParticleHandler.SpawnParticle(new SakuraNoteParticle(
                            target.Center + Main.rand.NextVector2Circular(30f, 30f),
                            noteVel, GetHeroicGradient(Main.rand.NextFloat()),
                            Main.rand.NextFloat(0.4f, 0.8f), Main.rand.Next(50, 80)));
                    }

                    // Heavy heroic smoke
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 smokeVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1f, 3f);
                        ValorParticleHandler.SpawnParticle(new HeroicSmokeParticle(
                            target.Center + Main.rand.NextVector2Circular(40f, 40f),
                            smokeVel, Color.Lerp(ScarletEmber, BlackSmoke, Main.rand.NextFloat()),
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
                    (int)(Projectile.damage * 0.25f),
                    Projectile.knockBack * 0.5f,
                    Projectile.owner,
                    3 + _comboStep * 2); // 3/5/7 crystals by phase
            }
        }

        #endregion

        #region On Kill

        public override void OnKill(int timeLeft)
        {
            Owner.fullRotation = 0f;
            Owner.CelestialValor().IsLunging = false;
        }

        #endregion
    }
}
