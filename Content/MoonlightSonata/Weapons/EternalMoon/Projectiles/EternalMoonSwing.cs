using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Primitives;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Utilities;
using MagnumOpus.Content.MoonlightSonata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Utilities.EternalMoonUtils;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Projectiles
{
    /// <summary>
    /// The Eternal Moon's swing projectile — "The Eternal Tide"
    /// 
    /// This is the central rendering and combat file for the Eternal Moon melee weapon.
    /// It handles all swing animation, collision, trail rendering, blade drawing, particle spawning,
    /// combo phase tracking, special mechanics, and sub-projectile creation.
    /// 
    /// --- COMBO SYSTEM: LUNAR CYCLE ---
    /// 5-phase combo that cycles through lunar phases with escalating VFX:
    ///   Phase 0: New Moon      — Subtle dark purple trail, minimal particles
    ///   Phase 1: Waxing        — Growing ice blue trail, crescent sparks
    ///   Phase 2: Half Moon     — Ghost reflection echoes appear offset from main swing
    ///   Phase 3: Waning        — Intense trail with tidal wave crests, music notes scatter
    ///   Phase 4: Full Moon     — Massive empowered slash, tidal detonation on hit, lifesteal
    /// 
    /// --- SPECIAL ATTACKS ---
    ///   Alt-click: Lunar Surge — Dash attack surfing on a wave of moonlight
    ///                           On hit: applies Lunar Stasis, spawns crescent slash VFX
    ///                           If a surge hit connects, next swing is empowered (Full Moon override)
    ///   
    ///   Full Moon Swing        — 1.5x scale, spawns TidalDetonation explosion on hit with lifesteal
    ///   
    ///   Half Moon Ghost        — Spawns 2 delayed ghost reflections that repeat the swing
    ///
    /// --- VISUAL LAYERS (render order) ---
    ///   0. Smear Overlay       — SwordSmearFoundation SmearDistortShader: 3-sublayer arc distortion
    ///   1. Tidal Glow Pass     — Wide, soft bloom underlayer using TidalTrailGlow shader
    ///   2. Tidal Trail Pass    — Core arc trail using TidalTrailMain shader with caustic highlights
    ///   3. Surge Trail         — During dash: trailing positions with TidalTrailGlow shader
    ///   4. Blade Sprite        — UV-rotated via SwingSprite shader + lens flare at tip
    ///   5. Crescent Bloom      — Procedural crescent moon overlay at blade tip (during Waxing+)
    /// </summary>
    public class EternalMoonSwing : ModProjectile
    {
        #region Constants and Properties

        public Player Owner => Main.player[Projectile.owner];

        private const float BladeLength = 160f;
        private const int BaseSwingTime = 72;
        private const float MaxSwingAngle = MathHelper.PiOver2 * 1.7f;
        private const float SurgeSpeed = 50f;
        private const float SurgePercentage = 0.55f;
        private const float FullMoonUpscale = 1.5f;
        private const float ReboundSpeed = 5f;
        private const int SurgeCooldown = 60 * 3;
        private const int FullMoonOpportunity = 37 * 3;
        private const float NotMeleeDamagePenalty = 0.3f;
        private const float DetonationDamageFactor = 2.0f;
        private const float TextureDrawScale = 0.136f;

        public int GetSwingTime
        {
            get
            {
                if (State == SwingState.LunarSurge)
                    return EternalMoon.SurgeDashTime * Projectile.extraUpdates;
                return BaseSwingTime;
            }
        }

        public float Timer => SwingTime - Projectile.timeLeft;
        public float Progression => Timer / (float)SwingTime;

        public float SurgeProgression => Progression < (1 - SurgePercentage)
            ? 0 : (Progression - (1 - SurgePercentage)) / SurgePercentage;

        #endregion

        #region State Machine

        public enum SwingState
        {
            Swinging,
            LunarSurge
        }

        public SwingState State
        {
            get => Projectile.ai[0] == 1 ? SwingState.LunarSurge : SwingState.Swinging;
            set => Projectile.ai[0] = (int)value;
        }

        public bool PerformingFullMoonSlash => Projectile.ai[0] > 1;

        public bool InPostSurgeStasis
        {
            get => Projectile.ai[1] > 0;
            set => Projectile.ai[1] = value ? 1 : 0;
        }

        public ref float SwingTime => ref Projectile.localAI[0];
        public ref float SquishFactor => ref Projectile.localAI[1];

        /// <summary>Current lunar phase for this swing (cached from player on init).</summary>
        private int _lunarPhase;

        /// <summary>Phase intensity 0.25→1.0 driving shader uniforms and VFX density.</summary>
        private float _phaseIntensity;

        public float IdealSize => PerformingFullMoonSlash ? FullMoonUpscale : 1f;

        #endregion

        #region Swing Animation Curves

        public int Direction => Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;
        public float BaseRotation => Projectile.velocity.ToRotation();
        public Vector2 SquishVector => new Vector2(1f + (1 - SquishFactor) * 0.5f, SquishFactor);

        // Swing curve: slow lunar windup → flowing tidal sweep → gentle moonlit overshoot
        public CurveSegment GentleRise = new(SineOutEasing, 0f, -1f, 0.25f, 2);
        public CurveSegment TidalSweep = new(PolyInEasing, 0.22f, -0.75f, 1.65f, 3);
        public CurveSegment MoonlitSettle = new(PolyOutEasing, 0.82f, 0.9f, 0.1f, 2);

        public float SwingAngleShiftAtProgress(float progress) =>
            State == SwingState.LunarSurge ? 0 :
            MaxSwingAngle * PiecewiseAnimation(progress, GentleRise, TidalSweep, MoonlitSettle);

        public float SwordRotationAtProgress(float progress) =>
            State == SwingState.LunarSurge ? BaseRotation :
            BaseRotation + SwingAngleShiftAtProgress(progress) * Direction;

        public float SquishAtProgress(float progress) =>
            State == SwingState.LunarSurge ? 1 :
            MathHelper.Lerp(SquishVector.X, SquishVector.Y,
                (float)Math.Abs(Math.Sin(SwingAngleShiftAtProgress(progress))));

        public Vector2 DirectionAtProgress(float progress) =>
            State == SwingState.LunarSurge ? Projectile.velocity :
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
            return (BaseRotation + angleShift * Direction).ToRotationVector2() * SquishAtProgress(progress);
        }

        // Surge dash curves
        public CurveSegment SurgeWindback = new(SineBumpEasing, 0f, -8f, -12f);
        public CurveSegment SurgeThrust => new(PolyOutEasing, 1 - SurgePercentage, -8, 10f, 4);
        public float SurgeDashDisplace => PiecewiseAnimation(Progression, SurgeWindback, SurgeThrust);

        #endregion

        #region Particle and Dust spawning

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

        #region Texture references

        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Weapons/EternalMoon/EternalMoon";
        private static Asset<Texture2D> _lensFlare;
        private static Asset<Texture2D> _bloomCircle;
        private static Asset<Texture2D> _noiseTexture;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _gradientRamp;

        // SmearDistortShader overlay (Foundation-based swing arc distortion)
        private static Asset<Texture2D> _smearArcTexture;
        private static Asset<Texture2D> _smearNoiseTex;
        private static Asset<Texture2D> _smearGradientTex;
        private static Effect _smearDistortShader;
        private static bool _smearShaderLoaded;

        #endregion

        #region Setup

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 100;
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
            writer.Write(_lunarPhase);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            SwingTime = reader.ReadSingle();
            SquishFactor = reader.ReadSingle();
            _lunarPhase = reader.ReadInt32();
        }

        #endregion

        #region Collision

        public override bool ShouldUpdatePosition() => State == SwingState.LunarSurge && !InPostSurgeStasis;

        public override bool? CanDamage()
        {
            if (State != SwingState.LunarSurge)
                return null;
            if (InPostSurgeStasis)
                return false;
            if (Projectile.timeLeft > SwingTime * SurgePercentage)
                return false;
            return null;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Projectile.Center;
            Vector2 end = start + SwordDirection * (BladeLength + 40) * Projectile.scale;
            float width = State == SwingState.LunarSurge ? Projectile.scale * 42f : Projectile.scale * 28f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref _);
        }

        #endregion

        #region Initialization

        public void InitializationEffects(bool startInit)
        {
            Projectile.velocity = Owner.MountedCenter.DirectionTo(Main.MouseWorld);
            SquishFactor = Main.rand.NextFloat(0.7f, 1f);

            // Cache lunar phase from player
            _lunarPhase = Owner.EternalMoon().LunarPhase;
            _phaseIntensity = GetPhaseIntensity(_lunarPhase);

            if (startInit && State != SwingState.LunarSurge)
                Projectile.scale = 0.02f;
            else
            {
                Projectile.scale = 1f;
                if (PerformingFullMoonSlash)
                {
                    State = SwingState.Swinging;
                    _lunarPhase = 4;
                    _phaseIntensity = 1f;
                }
            }

            if (PerformingFullMoonSlash)
                SquishFactor = 0.72f;

            SwingTime = GetSwingTime;
            Projectile.timeLeft = (int)SwingTime;
            Projectile.netUpdate = true;
        }

        #endregion

        #region AI

        public override void AI()
        {
            if (InPostSurgeStasis || Projectile.timeLeft == 0)
                return;

            if (Projectile.timeLeft >= 9999 || (Projectile.timeLeft == 1 && Owner.channel && State != SwingState.LunarSurge))
                InitializationEffects(Projectile.timeLeft >= 9999);

            switch (State)
            {
                case SwingState.Swinging:
                    DoBehavior_Swinging();
                    break;
                case SwingState.LunarSurge:
                    DoBehavior_LunarSurge();
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

            // Surge cooldown freeze
            if (Projectile.timeLeft == 1 && State == SwingState.LunarSurge && !InPostSurgeStasis)
            {
                Projectile.timeLeft = SurgeCooldown;
                InPostSurgeStasis = true;
                Owner.fullRotation = 0f;
                Owner.EternalMoon().SurgingForward = false;
            }
        }

        #endregion

        #region Swing Behavior

        public void DoBehavior_Swinging()
        {
            // Play swing sound at 20% through
            if (Projectile.timeLeft == (int)(SwingTime / 5))
            {
                SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.7f, PitchVariance = 0.3f }, Projectile.Center);
                if (PerformingFullMoonSlash)
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.5f, Pitch = -0.3f }, Projectile.Center);
            }

            // Dynamic moonlight lighting along the blade
            Vector3 lightColor = Color.Lerp(DarkPurple, IceBlue, (float)Math.Pow(Progression, 2)).ToVector3();
            lightColor *= 1.2f * _phaseIntensity * (float)Math.Sin(Progression * MathHelper.Pi);
            Lighting.AddLight(Owner.MountedCenter + SwordDirection * 90, lightColor);

            // Scale up to ideal size
            if (Projectile.scale < IdealSize)
                Projectile.scale = MathHelper.Lerp(Projectile.scale, IdealSize, 0.08f);

            // Shrink near end of slash
            if (!Owner.channel && Progression > 0.7f)
                Projectile.scale = (0.5f + 0.5f * (float)Math.Pow(1 - (Progression - 0.7f) / 0.3f, 0.5)) * IdealSize;

            // === DUST SPAWNING ===
            // Tidal dust from blade edge
            if (Main.rand.NextFloat() * 3f < DustRisk)
            {
                Vector2 dustPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale *
                    (float)Math.Pow(Main.rand.NextFloat(0.5f, 1f), 0.5f);
                Dust tidalDust = Dust.NewDustPerfect(dustPos, ModContent.DustType<TidalDust>(),
                    SwordDirection.RotatedBy(-MathHelper.PiOver2 * Direction) * 2f);
                tidalDust.noGravity = true;
                tidalDust.alpha = 10;
                tidalDust.scale = 0.5f * _phaseIntensity;
            }

            // Purple/ice blue ambient dust
            if (Main.rand.NextFloat() < DustRisk * 0.7f)
            {
                Color dustColor = Color.Lerp(Violet, IceBlue, Main.rand.NextFloat());
                Vector2 dustPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale *
                    (float)Math.Pow(Main.rand.NextFloat(0.2f, 1f), 0.5f);
                Dust lunarDust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch,
                    SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction) * 2.2f, 0, dustColor);
                lunarDust.scale = 0.4f;
                lunarDust.fadeIn = Main.rand.NextFloat() * 1.0f;
                lunarDust.noGravity = true;
            }

            // === PARTICLE SPAWNING (phase-dependent) ===
            SpawnSwingParticles();

            // === TIDAL WAVE PROJECTILES ===
            // Spawn homing tidal waves during the fast part of the swing
            int waveShootStart = (int)(SwingTime * 0.55f);
            int waveShootPeriod = (int)(SwingTime * 0.35f);
            int waveShootEnd = waveShootStart + waveShootPeriod;
            int wavesPerSwing = 2 + _lunarPhase; // 2-6 waves depending on phase
            int waveInterval = Math.Max(1, waveShootPeriod / Math.Max(1, wavesPerSwing - 1));

            if (Main.myPlayer == Projectile.owner && Timer >= waveShootStart && Timer < waveShootEnd &&
                (Timer - waveShootStart) % waveInterval == 0)
            {
                int waveDamage = (int)(Projectile.damage * NotMeleeDamagePenalty);
                Vector2 waveVelocity = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4 * 0.3);
                waveVelocity *= Owner.HeldItem.shootSpeed;

                Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                    Projectile.Center + waveVelocity * 5f, waveVelocity,
                    ModContent.ProjectileType<EternalMoonWave>(), waveDamage,
                    Projectile.knockBack / 3f, Projectile.owner, _lunarPhase);
            }

            // === HALF MOON GHOST REFLECTION ===
            // Spawn a single orbiting ghost blade above the player at Half Moon+
            if (_lunarPhase >= 2 && Progression > 0.3f && Progression < 0.35f && Main.myPlayer == Projectile.owner)
            {
                int ghostDamage = (int)(Projectile.damage * 0.4f);
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity,
                    ModContent.ProjectileType<EternalMoonGhost>(), ghostDamage, Projectile.knockBack * 0.3f,
                    Projectile.owner, _lunarPhase, 1);
            }

            // === ECHOING TIDES (every 4th swing) ===
            // Every 4th swing spawns 2 orbiting ghost echoes above the player
            if (Owner.EternalMoon().ShouldEchoTides && Progression > 0.25f && Progression < 0.3f && Main.myPlayer == Projectile.owner)
            {
                for (int echo = 0; echo < 2; echo++)
                {
                    int echoDamage = (int)(Projectile.damage * 0.25f);
                    int side = echo == 0 ? 1 : -1;
                    int proj = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity,
                        ModContent.ProjectileType<EternalMoonGhost>(), echoDamage, Projectile.knockBack * 0.2f,
                        Projectile.owner, _lunarPhase, side);
                    if (Main.projectile.IndexInRange(proj))
                        Main.projectile[proj].timeLeft -= echo * 8; // Staggered timing
                }

                // Echoing tides VFX — pulse rings
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        LunarParticleHandler.SpawnParticle(new TidalPhaseRingParticle(
                            Owner.MountedCenter, 0.8f + i * 0.3f,
                            Color.Lerp(IceBlue, Violet, i / 2f) * 0.6f, 20 + i * 5));
                    }
                    MoonlightVFXLibrary.SpawnMusicNotes(Owner.MountedCenter, count: 3, spread: 25f,
                        minScale: 0.5f, maxScale: 0.8f, lifetime: 40);
                }
            }

            // Advance phase on first swing frame
            if (Timer == 1)
                Owner.EternalMoon().AdvancePhase();
        }

        private void SpawnSwingParticles()
        {
            if (Main.dedServ) return;

            float tipX = BladeLength * Projectile.scale;
            Vector2 tipPos = Owner.MountedCenter + SwordDirection * tipX;
            var emPlayer = Owner.EternalMoon();
            float tidalMult = emPlayer.TidalPhaseMultiplier;

            // Tidal motes alongside the blade — density scales with phase AND tidal meter
            if (Main.rand.NextFloat() < 0.3f * _phaseIntensity * tidalMult && Progression > 0.2f && Progression < 0.85f)
            {
                Vector2 moteVel = SwordDirection.RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 3f) * tidalMult;
                Color moteColor = Color.Lerp(IceBlue, CrescentGlow, Main.rand.NextFloat());
                LunarParticleHandler.SpawnParticle(new TidalMoteParticle(
                    tipPos + Main.rand.NextVector2Circular(15f, 15f), moteVel,
                    Main.rand.NextFloat(0.3f, 0.7f) * _phaseIntensity * tidalMult, moteColor,
                    Main.rand.Next(25, 50)));
            }

            // Crescent sparks on fast part of swing (Phase 1+)
            if (_lunarPhase >= 1 && Progression > 0.4f && Progression < 0.8f && Main.rand.NextBool(4))
            {
                Vector2 sparkVel = SwordDirection.RotatedByRandom(0.3f) * Main.rand.NextFloat(4f, 8f);
                LunarParticleHandler.SpawnParticle(new CrescentSparkParticle(
                    tipPos, sparkVel, Main.rand.NextFloat(0.4f, 0.8f),
                    CrescentGlow, Main.rand.Next(15, 25)));
            }

            // Moon glint sparkles at blade tip (Phase 1+)
            if (_lunarPhase >= 1 && Progression > 0.35f && Progression < 0.75f && Main.rand.NextBool(8))
            {
                LunarParticleHandler.SpawnParticle(new MoonGlintParticle(
                    tipPos + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextFloat(0.3f, 0.5f) * _phaseIntensity,
                    EternalMoonUtils.MoonWhite, Main.rand.Next(15, 25)));
            }

            // Tidal droplets — water-like particles falling from swing arc (Phase 2+, Flood+)
            if (emPlayer.TidalPhase >= 1 && Progression > 0.3f && Progression < 0.8f && Main.rand.NextBool(5))
            {
                float bladePos = Main.rand.NextFloat(0.4f, 1f);
                Vector2 dropPos = Owner.MountedCenter + SwordDirection * BladeLength * bladePos * Projectile.scale;
                Vector2 dropVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1f, 0.5f));
                Color dropColor = Color.Lerp(IceBlue, MoonWhite, Main.rand.NextFloat(0.3f));
                LunarParticleHandler.SpawnParticle(new TidalDropletParticle(
                    dropPos, dropVel, Main.rand.NextFloat(0.3f, 0.6f),
                    dropColor, Main.rand.Next(20, 35)));
            }

            // Wave spray burst on swing peak (High Tide+)
            if (emPlayer.TidalPhase >= 2 && Math.Abs(Progression - 0.55f) < 0.03f)
            {
                int sprayCount = 6 + emPlayer.TidalPhase * 3;
                for (int i = 0; i < sprayCount; i++)
                {
                    Vector2 sprayVel = SwordDirection.RotatedByRandom(0.6f) * Main.rand.NextFloat(3f, 8f);
                    Color sprayColor = Color.Lerp(MoonWhite, IceBlue, Main.rand.NextFloat());
                    LunarParticleHandler.SpawnParticle(new WaveSprayParticle(
                        tipPos + Main.rand.NextVector2Circular(10f, 10f),
                        sprayVel, Main.rand.NextFloat(0.3f, 0.6f),
                        sprayColor, Main.rand.Next(10, 20)));
                }
            }

            // Tidal phase ring pulse — shows the tidal meter building
            if (emPlayer.TidalPhase >= 1 && Progression > 0.5f && Progression < 0.55f)
            {
                Color phaseColor = EternalMoonPlayer.TidalPhaseColors[emPlayer.TidalPhase];
                float ringScale = 0.5f + emPlayer.TidalPhase * 0.3f;
                LunarParticleHandler.SpawnParticle(new TidalPhaseRingParticle(
                    Owner.MountedCenter, ringScale, phaseColor, 25));
            }

            // Music notes scatter (Phase 3+)
            if (_lunarPhase >= 3 && Progression > 0.35f && Progression < 0.75f && Main.rand.NextBool(6))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                LunarParticleHandler.SpawnParticle(new LunarNoteParticle(
                    tipPos + Main.rand.NextVector2Circular(20f, 20f), noteVel,
                    Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(40, 70)));
            }

            // Full Moon bloom pulse at swing peak + hue-shifting music notes
            if (_lunarPhase == 4 && Math.Abs(Progression - 0.55f) < 0.02f)
            {
                LunarParticleHandler.SpawnParticle(new LunarBloomParticle(
                    tipPos, 0.4f, IceBlue, 25, 0.04f));
                LunarParticleHandler.SpawnParticle(new LunarBloomParticle(
                    Owner.MountedCenter, 0.6f, DarkPurple, 30, 0.025f));

                // Musical identity: visible hue-shifting notes burst from the Full Moon crescendo
                MoonlightVFXLibrary.SpawnMusicNotes(tipPos, count: 4, spread: 30f, minScale: 0.8f, maxScale: 1.1f, lifetime: 45);
            }

            // Phase 3+ swing: periodic music notes float from blade arc
            if (_lunarPhase >= 3 && Progression > 0.4f && Progression < 0.7f && (int)(Timer * 10) % 7 == 0)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(tipPos, count: 1, spread: 15f, minScale: 0.6f, maxScale: 0.85f, lifetime: 35);
            }

            // Tsunami phase: dense tidal smoke + additional wave spray continuously
            if (emPlayer.IsTsunami && Progression > 0.3f && Progression < 0.8f && Main.rand.NextBool(3))
            {
                Vector2 smokePos = tipPos + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 smokeVel = SwordDirection.RotatedByRandom(1f) * Main.rand.NextFloat(1f, 2.5f);
                LunarParticleHandler.SpawnParticle(new TidalSmokeParticle(
                    smokePos, smokeVel, Main.rand.NextFloat(0.2f, 0.4f),
                    Color.Lerp(DarkPurple, IceBlue, Main.rand.NextFloat()),
                    Main.rand.Next(30, 50)));
            }
        }

        #endregion

        #region Lunar Surge (Dash) Behavior

        public void DoBehavior_LunarSurge()
        {
            Owner.mount?.Dismount(Owner);
            Owner.RemoveAllGrapplingHooks();

            if (SurgeProgression == 0)
            {
                // Sound cue before dash
                if (Projectile.timeLeft == 1 + (int)(SwingTime * SurgePercentage))
                    SoundEngine.PlaySound(SoundID.Item66 with { Volume = 0.7f, Pitch = 0.2f }, Projectile.Center);

                Projectile.velocity = Owner.MountedCenter.DirectionTo(Main.MouseWorld);
                Projectile.oldPos = new Vector2[Projectile.oldPos.Length];
                for (int i = 0; i < Projectile.oldPos.Length; ++i)
                    Projectile.oldPos[i] = Projectile.position;
            }
            else
            {
                // Gentle course correction during surge
                float correctionStrength = MathHelper.PiOver4 * 0.04f * (float)Math.Pow(SurgeProgression, 3);
                float currentRotation = Projectile.velocity.ToRotation();
                float idealRotation = Owner.MountedCenter.DirectionTo(Main.MouseWorld).ToRotation();
                Projectile.velocity = currentRotation.AngleTowards(idealRotation, correctionStrength).ToRotationVector2();

                Owner.fallStart = (int)(Owner.position.Y / 16f);

                float velocityPower = (float)Math.Sin(MathHelper.Pi * SurgeProgression);
                velocityPower = (float)Math.Pow(Math.Abs(velocityPower), 0.6f);
                Vector2 newVelocity = Projectile.velocity * SurgeSpeed * (0.2f + 0.8f * velocityPower);
                Owner.velocity = newVelocity;
                Owner.EternalMoon().SurgingForward = true;

                // Tidal dust during surge
                if (Main.rand.NextBool())
                {
                    Dust d = Dust.NewDustPerfect(Owner.MountedCenter + Main.rand.NextVector2Circular(20f, 20f),
                        ModContent.DustType<TidalDust>(), SwordDirection * -2.6f);
                    d.scale = 0.5f;
                    d.noGravity = true;
                }

                // Tidal mote particles during surge
                if (Main.rand.NextBool(4) && SurgeProgression < 0.85f && !Main.dedServ)
                {
                    Vector2 particleSpeed = SwordDirection * -1 * Main.rand.NextFloat(5f, 9f);
                    LunarParticleHandler.SpawnParticle(new TidalMoteParticle(
                        Owner.MountedCenter + Main.rand.NextVector2Circular(20f, 20f) + Owner.velocity * 4,
                        particleSpeed, Main.rand.NextFloat(0.3f, 0.6f), IceBlue, 30));
                }

                // Crescent sparks trailing the surge
                if (Main.rand.NextBool(5) && !Main.dedServ)
                {
                    Vector2 sparkSpeed = SwordDirection * -1 * Main.rand.NextFloat(6f, 10f);
                    LunarParticleHandler.SpawnParticle(new CrescentSparkParticle(
                        Owner.MountedCenter + Main.rand.NextVector2Circular(30f, 30f),
                        sparkSpeed, Main.rand.NextFloat(0.4f, 0.7f), CrescentGlow, 20));
                }

                // Moonlight along surge path
                Lighting.AddLight(Owner.MountedCenter, IceBlue.ToVector3() * 0.8f);
            }

            // Stop the surge on last frame
            if (Projectile.timeLeft == 1)
                Owner.velocity *= 0.15f;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 * Direction;
        }

        #endregion

        #region Trail Width/Color Functions

        public float SlashWidthFunction(float completionRatio, Vector2 vertexPos)
        {
            float tidalMult = Owner.EternalMoon().TidalPhaseMultiplier;
            return SquishAtProgress(RealProgressionAtTrailCompletion(completionRatio)) * Projectile.scale * 55f * _phaseIntensity * tidalMult;
        }

        public Color SlashColorFunction(float completionRatio, Vector2 vertexPos)
        {
            float fade = Utils.GetLerpValue(0.9f, 0.35f, completionRatio, true);
            Color baseColor = Color.Lerp(DarkPurple, IceBlue, completionRatio * 0.6f + _phaseIntensity * 0.2f);
            baseColor.A = 0; // Additive-ready
            return baseColor * fade * Projectile.Opacity;
        }

        public float GlowWidthFunction(float completionRatio, Vector2 vertexPos) =>
            SlashWidthFunction(completionRatio, vertexPos) * (1.5f + Owner.EternalMoon().TidalPhase * 0.15f);

        public Color GlowColorFunction(float completionRatio, Vector2 vertexPos)
        {
            float fade = Utils.GetLerpValue(0.95f, 0.3f, completionRatio, true);
            Color glowColor = Color.Lerp(NightPurple, Violet, completionRatio * 0.5f);
            glowColor.A = 0;
            return glowColor * fade * 0.5f * Projectile.Opacity;
        }

        public float SurgeWidthFunction(float completionRatio, Vector2 vertexPos)
        {
            float width = Utils.GetLerpValue(0f, 0.2f, completionRatio, true) * Projectile.scale * 45f;
            width *= (1 - (float)Math.Pow(SurgeProgression, 5));
            return width;
        }

        public Color SurgeColorFunction(float completionRatio, Vector2 vertexPos)
        {
            Color c = Color.Lerp(IceBlue, CrescentGlow, completionRatio * 0.5f);
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
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            if (Projectile.Opacity <= 0f || InPostSurgeStasis)
                return false;

            DrawSmearOverlay();
            DrawTidalGlow();
            DrawTidalTrail();
            DrawSurgeTrail();
            DrawBlade();
            DrawCrescentBloom();
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        /// <summary>
        /// Layer 0 (Foundation): SmearDistortShader overlay from SwordSmearFoundation.
        /// Renders the SwordArcSmear texture with fluid distortion shader in 3 sub-layers
        /// (outer glow → main body → bright core), using Moonlight Sonata palette.
        /// Parameters scale with tidal phase: distortStrength 0.08→0.12, flowSpeed 0.4→0.8.
        /// </summary>
        public void DrawSmearOverlay()
        {
            if (State != SwingState.Swinging || Progression < 0.25f)
                return;

            // Lazy-load smear shader and textures
            if (!_smearShaderLoaded)
            {
                _smearShaderLoaded = true;
                try
                {
                    _smearDistortShader = ModContent.Request<Effect>(
                        "MagnumOpus/Content/FoundationWeapons/SwordSmearFoundation/Shaders/SmearDistortShader",
                        AssetRequestMode.ImmediateLoad).Value;
                }
                catch { _smearDistortShader = null; }
            }

            _smearArcTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/SlashArcs/SwordArcSmear");
            _smearNoiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileableFBMNoise");
            _smearGradientTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/MoonlightSonataGradientLUTandRAMP");

            Texture2D smearTex = _smearArcTexture.Value;
            Vector2 smearOrigin = smearTex.Size() / 2f;
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawOrigin = Owner.MountedCenter - Main.screenPosition;

            // Scale smear to match blade length (doc: BladeLength=100f visual scale for smear)
            float maxDim = MathF.Max(smearTex.Width, smearTex.Height);
            float smearScale = (BladeLength * 2.2f) / maxDim;
            if (PerformingFullMoonSlash)
                smearScale *= FullMoonUpscale;

            // Rotation follows the current swing angle
            float smearRotation = SwordRotation + (Direction < 0 ? MathHelper.Pi : 0f);

            // Fade envelope: smooth in after 0.25, sustain, fade at tail
            float smearAlpha;
            if (Progression < 0.35f)
                smearAlpha = (Progression - 0.25f) / 0.10f;
            else if (Progression > 0.85f)
                smearAlpha = (1f - Progression) / 0.15f;
            else
                smearAlpha = 1f;
            smearAlpha = MathHelper.Clamp(smearAlpha, 0f, 1f) * _phaseIntensity;

            // Tidal phase scaling: distort and flow intensify with tidal phase
            float tidalMult = 1f + _phaseIntensity * 0.5f;
            float baseDistort = MathHelper.Lerp(0.08f, 0.12f, _phaseIntensity);
            float flowSpeed = MathHelper.Lerp(0.4f, 0.8f, _phaseIntensity);

            if (_smearDistortShader != null)
            {
                // --- SHADER PATH: fluid distortion + gradient coloring (Moonlight palette) ---
                // NOTE: Uses BlendState.Additive (SourceAlpha) here, NOT TrueAdditive,
                // because the SwordArcSmear texture uses alpha transparency to define
                // its arc shape. TrueAdditive ignores alpha → full rectangle drawn.
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                    SamplerState.LinearWrap, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.EffectMatrix);

                float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;

                _smearDistortShader.Parameters["uTime"]?.SetValue(time);
                _smearDistortShader.Parameters["fadeAlpha"]?.SetValue(smearAlpha);
                _smearDistortShader.Parameters["flowSpeed"]?.SetValue(flowSpeed);
                _smearDistortShader.Parameters["noiseScale"]?.SetValue(2.5f);
                _smearDistortShader.Parameters["noiseTex"]?.SetValue(_smearNoiseTex.Value);
                _smearDistortShader.Parameters["gradientTex"]?.SetValue(_smearGradientTex.Value);

                // Sub-layer A: Wide outer tidal glow (stronger distortion)
                _smearDistortShader.Parameters["distortStrength"]?.SetValue(baseDistort * tidalMult);
                _smearDistortShader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * smearAlpha * 0.45f,
                    smearRotation, smearOrigin,
                    smearScale * 1.15f, SpriteEffects.None, 0f);

                // Sub-layer B: Main smear body (medium distortion)
                _smearDistortShader.Parameters["distortStrength"]?.SetValue(baseDistort * 0.625f * tidalMult);
                _smearDistortShader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * smearAlpha * 0.75f,
                    smearRotation, smearOrigin,
                    smearScale, SpriteEffects.None, 0f);

                // Sub-layer C: Bright core (subtle distortion, sharper detail)
                _smearDistortShader.Parameters["distortStrength"]?.SetValue(baseDistort * 0.3125f * tidalMult);
                _smearDistortShader.CurrentTechnique.Passes[0].Apply();
                sb.Draw(smearTex, drawOrigin, null,
                    Color.White * smearAlpha * 0.6f,
                    smearRotation, smearOrigin,
                    smearScale * 0.85f, SpriteEffects.None, 0f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                // --- FALLBACK: NEON RED layers (shader failed!) ---
                // NOTE: Same as shader path — must use SourceAlpha additive for alpha-masked arc textures.
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.EffectMatrix);

                Color _neonRed = new Color(255, 0, 50);
                sb.Draw(smearTex, drawOrigin, null,
                    _neonRed * smearAlpha * 0.35f,
                    smearRotation, smearOrigin,
                    smearScale * 1.15f, SpriteEffects.None, 0f);

                sb.Draw(smearTex, drawOrigin, null,
                    _neonRed * smearAlpha * 0.65f,
                    smearRotation, smearOrigin,
                    smearScale, SpriteEffects.None, 0f);

                sb.Draw(smearTex, drawOrigin, null,
                    _neonRed * smearAlpha * 0.5f,
                    smearRotation, smearOrigin,
                    smearScale * 0.85f, SpriteEffects.None, 0f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        /// <summary>Layer 1: Wide, soft tidal glow underlayer.</summary>
        public void DrawTidalGlow()
        {
            if (State != SwingState.Swinging || Progression < 0.4f)
                return;

            global::MagnumOpus.Common.Systems.MagnumDrawingUtils.EnterShaderRegion(Main.spriteBatch, MagnumBlendStates.TrueAdditive);

            var shader = GameShaders.Misc["MagnumOpus:EternalMoonTidalGlow"];
            _noiseTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/MusicalWavePattern");
            _gradientRamp ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/MoonlightSonataGradientLUTandRAMP");

            shader.UseImage1(_noiseTexture);
            shader.UseImage2(_gradientRamp);
            shader.UseColor(DarkPurple);
            shader.UseSecondaryColor(IceBlue);
            shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 1.5f);
            shader.Shader.Parameters["uIntensity"]?.SetValue(_phaseIntensity);
            shader.Shader.Parameters["uOpacity"]?.SetValue(0.35f * _phaseIntensity);
            shader.Shader.Parameters["uOverbrightMult"]?.SetValue(1.2f + _phaseIntensity * 0.5f);
            shader.Shader.Parameters["uScrollSpeed"]?.SetValue(1.0f);
            shader.Shader.Parameters["uDistortionAmt"]?.SetValue(0.06f * _phaseIntensity);
            shader.Shader.Parameters["uHasSecondaryTex"]?.SetValue(1.0f);
            shader.Shader.Parameters["uSecondaryTexScale"]?.SetValue(2.0f);
            shader.Shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.3f);
            shader.Apply();

            LunarTrailRenderer.RenderTrail(GenerateSlashPoints(), new(
                GlowWidthFunction, GlowColorFunction,
                (_, _) => Projectile.Center,
                shader: shader), 40);

            global::MagnumOpus.Common.Systems.MagnumDrawingUtils.ExitShaderRegion(Main.spriteBatch);
        }

        /// <summary>Layer 2: Core tidal trail with caustic highlights and standing wave crests.</summary>
        public void DrawTidalTrail()
        {
            if (State != SwingState.Swinging || Progression < 0.42f)
                return;

            global::MagnumOpus.Common.Systems.MagnumDrawingUtils.EnterShaderRegion(Main.spriteBatch);

            var shader = GameShaders.Misc["MagnumOpus:EternalMoonTidalTrail"];
            _noiseTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/MusicalWavePattern");
            _gradientRamp ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/MoonlightSonataGradientLUTandRAMP");

            shader.UseImage1(_noiseTexture);
            shader.UseImage2(_gradientRamp);
            shader.UseColor(DarkPurple);
            shader.UseSecondaryColor(IceBlue);
            shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
            shader.Shader.Parameters["uIntensity"]?.SetValue(_phaseIntensity);
            shader.Shader.Parameters["uOpacity"]?.SetValue(1.0f);
            shader.Shader.Parameters["uOverbrightMult"]?.SetValue(2.5f + _phaseIntensity * 0.5f);
            shader.Shader.Parameters["uScrollSpeed"]?.SetValue(1.2f);
            shader.Shader.Parameters["uDistortionAmt"]?.SetValue(0.08f + _phaseIntensity * 0.04f);
            shader.Shader.Parameters["uHasSecondaryTex"]?.SetValue(1.0f);
            shader.Shader.Parameters["uNoiseScale"]?.SetValue(1.5f);
            shader.Shader.Parameters["uSecondaryTexScale"]?.SetValue(3.0f);
            shader.Shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);
            shader.Apply();

            LunarTrailRenderer.RenderTrail(GenerateSlashPoints(), new(
                SlashWidthFunction, SlashColorFunction,
                (_, _) => Projectile.Center,
                shader: shader), 40);

            global::MagnumOpus.Common.Systems.MagnumDrawingUtils.ExitShaderRegion(Main.spriteBatch);
        }

        /// <summary>Layer 3: Surge (dash) trail using trailing positions.</summary>
        public void DrawSurgeTrail()
        {
            if (State != SwingState.LunarSurge)
                return;

            global::MagnumOpus.Common.Systems.MagnumDrawingUtils.EnterShaderRegion(Main.spriteBatch, MagnumBlendStates.TrueAdditive);

            var shader = GameShaders.Misc["MagnumOpus:EternalMoonSurgeTrail"];
            _noiseTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/MusicalWavePattern");
            _gradientRamp ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/MoonlightSonataGradientLUTandRAMP");

            // Tidal color cycling during surge
            Color mainColor = Color.Lerp(IceBlue, CrescentGlow,
                (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);
            Color secondaryColor = Color.Lerp(Violet, IceBlue,
                (float)Math.Cos(Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);

            shader.UseImage1(_noiseTexture);
            shader.UseImage2(_gradientRamp);
            shader.UseColor(mainColor);
            shader.UseSecondaryColor(secondaryColor);
            shader.Shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
            shader.Shader.Parameters["uIntensity"]?.SetValue(0.8f);
            shader.Shader.Parameters["uOpacity"]?.SetValue(0.9f);
            shader.Shader.Parameters["uOverbrightMult"]?.SetValue(2.5f);
            shader.Shader.Parameters["uScrollSpeed"]?.SetValue(1.5f);
            shader.Shader.Parameters["uDistortionAmt"]?.SetValue(0.1f);
            shader.Shader.Parameters["uHasSecondaryTex"]?.SetValue(1.0f);
            shader.Shader.Parameters["uSecondaryTexScale"]?.SetValue(2.5f);
            shader.Shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.4f);
            shader.Apply();

            Vector2 trailOffset = (Projectile.rotation - Direction * MathHelper.PiOver4).ToRotationVector2() * 80f + Projectile.Size * 0.5f;
            var positionsToUse = Projectile.oldPos.Take(50).ToArray();

            LunarTrailRenderer.RenderTrail(positionsToUse, new(
                SurgeWidthFunction, SurgeColorFunction,
                (_, _) => trailOffset,
                shader: shader), 25);

            global::MagnumOpus.Common.Systems.MagnumDrawingUtils.ExitShaderRegion(Main.spriteBatch);
        }

        /// <summary>Layer 4: Blade sprite with UV rotation shader + lens flare at tip.</summary>
        public void DrawBlade()
        {
            var texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            SpriteEffects direction = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (State == SwingState.Swinging)
            {
                // Apply UV rotation shader for the blade sprite
                Effect swingFX = Filters.Scene["MagnumOpus:EternalMoonSwingSprite"].GetShader().Shader;
                swingFX.Parameters["rotation"]?.SetValue(SwingAngleShift + MathHelper.PiOver4 + (Direction == -1 ? MathHelper.Pi : 0f));
                swingFX.Parameters["pommelToOriginPercent"]?.SetValue(0.05f);
                swingFX.Parameters["color"]?.SetValue(Color.White.ToVector4());

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, swingFX,
                    Main.GameViewMatrix.TransformationMatrix);

                Main.EntitySpriteDraw(texture, Owner.MountedCenter - Main.screenPosition, null,
                    Color.White, BaseRotation, texture.Size() / 2f,
                    SquishVector * 2.8f * Projectile.scale * TextureDrawScale, direction, 0);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null,
                    Main.GameViewMatrix.TransformationMatrix);

                // === LENS FLARE AT BLADE TIP ===
                _lensFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar");
                Texture2D shineTex = _lensFlare.Value;
                Vector2 shineScale = new Vector2(1f, 2.5f);

                float flareOpacity = (Progression < 0.25f ? 0f : 0.15f + 0.85f * (float)Math.Sin(MathHelper.Pi * (Progression - 0.25f) / 0.75f))
                    * 0.5f * _phaseIntensity;
                Color flareColor = Color.Lerp(Violet, CrescentGlow, (float)Math.Pow(Progression, 2));
                flareColor.A = 0;

                Main.EntitySpriteDraw(shineTex,
                    Owner.MountedCenter + DirectionAtProgressSmoothed(Progression) * Projectile.scale * BladeLength - Main.screenPosition,
                    null, flareColor * flareOpacity, MathHelper.PiOver2,
                    shineTex.Size() / 2f, shineScale * Projectile.scale, 0, 0);

                // Second flare rotated perpendicular for cross-star effect
                Main.EntitySpriteDraw(shineTex,
                    Owner.MountedCenter + DirectionAtProgressSmoothed(Progression) * Projectile.scale * BladeLength - Main.screenPosition,
                    null, flareColor * flareOpacity * 0.6f, 0f,
                    shineTex.Size() / 2f, shineScale * Projectile.scale * 0.7f, 0, 0);
            }
            else
            {
                // During Lunar Surge: standard sprite draw with energy glow copies
                float rotation = BaseRotation + MathHelper.PiOver4;
                Vector2 origin = new Vector2(0, texture.Height);
                Vector2 drawPosition = Projectile.Center + Projectile.velocity * Projectile.scale * SurgeDashDisplace - Main.screenPosition;

                if (Direction == -1)
                {
                    rotation += MathHelper.PiOver2;
                    origin.X = texture.Width;
                }

                Projectile.scale = MathHelper.Lerp(1f, 0.25f, MathF.Pow(SurgeProgression, 6));

                Main.EntitySpriteDraw(texture, drawPosition, null, Color.White, rotation, origin, Projectile.scale * TextureDrawScale, direction, 0);

                // Additive energy glow copies (ice blue moonlight)
                float energyPower = Utils.GetLerpValue(0f, 0.3f, Progression, true) * Utils.GetLerpValue(1f, 0.85f, Progression, true);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 4f + BaseRotation).ToRotationVector2() * energyPower * Projectile.scale * 6f;
                    Color glowColor = Color.Lerp(IceBlue, CrescentGlow, Progression);
                    glowColor.A = 0;
                    Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null,
                        glowColor * 0.14f, rotation, origin, Projectile.scale * TextureDrawScale, direction, 0);
                }
            }
        }

        /// <summary>Layer 5: Multi-layer crescent bloom overlay at blade tip (Waxing phase+).
        /// Uses SoftRadialBloom for gentle outer halos and PointBloom for sharp inner core,
        /// with palette-driven color interpolation for richer tidal gradients.</summary>
        public void DrawCrescentBloom()
        {
            if (State != SwingState.Swinging || _lunarPhase < 1 || Progression < 0.3f || Progression > 0.85f)
                return;

            _bloomCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            Texture2D sharpBloom = _bloomCircle.Value;
            Texture2D softBloom = _softRadialBloom.Value;

            Vector2 tipPos = Owner.MountedCenter + DirectionAtProgressSmoothed(Progression) * Projectile.scale * BladeLength - Main.screenPosition;

            float crescentScale = 0.4f + 0.3f * _phaseIntensity;
            float crescentOpacity = (float)Math.Sin(MathHelper.Pi * (Progression - 0.3f) / 0.55f) * 0.12f * _phaseIntensity;

            // Palette-driven tidal color: shifts from deep purple at swing start to ice blue at peak
            float paletteT = 0.2f + Progression * 0.6f;
            Color tidalOuter = GetLunarGradient(paletteT - 0.15f);
            Color tidalInner = GetLunarGradient(paletteT + 0.15f);

            // Layer 1: Wide soft radial halo (SoftRadialBloom) — the moon's atmospheric glow
            tidalOuter.A = 0;
            Main.spriteBatch.Draw(softBloom, tipPos, null, tidalOuter * crescentOpacity * 0.15f,
                0f, softBloom.Size() / 2f, crescentScale * 0.18f * Projectile.scale, SpriteEffects.None, 0f);

            // Layer 2: Mid-range soft glow (SoftRadialBloom) — violet body
            Color midColor = Violet with { A = 0 };
            Main.spriteBatch.Draw(softBloom, tipPos, null, midColor * crescentOpacity * 0.2f,
                SwordRotation * 0.5f, softBloom.Size() / 2f, crescentScale * 0.12f * Projectile.scale, SpriteEffects.None, 0f);

            // Layer 3: Inner crescent core (PointBloom) — bright ice blue
            tidalInner.A = 0;
            Main.spriteBatch.Draw(sharpBloom, tipPos, null, tidalInner * crescentOpacity,
                SwordRotation, sharpBloom.Size() / 2f, crescentScale * 0.06f * Projectile.scale, SpriteEffects.None, 0f);

            // Layer 4: White-hot center (PointBloom) — moon zenith
            Color coreWhite = MoonWhite with { A = 0 };
            Main.spriteBatch.Draw(sharpBloom, tipPos, null, coreWhite * crescentOpacity * 0.6f,
                SwordRotation, sharpBloom.Size() / 2f, crescentScale * 0.025f * Projectile.scale, SpriteEffects.None, 0f);

            // === THEME-SPECIFIC BLOOM LAYERS ===
            // Layer 5: MS Star Flare — sharp 4-pointed flare at blade tip
            var msStarFlare = LunarThemeTextures.MSStarFlare;
            if (msStarFlare != null)
            {
                Color flareColor = tidalInner with { A = 0 };
                Main.spriteBatch.Draw(msStarFlare, tipPos, null, flareColor * crescentOpacity * 0.45f,
                    SwordRotation * 0.3f, msStarFlare.Size() / 2f, crescentScale * 0.35f * Projectile.scale, SpriteEffects.None, 0f);
            }

            // Layer 6: MS Glow Orb — soft ethereal bloom overlay behind the crescent
            var msGlowOrb = LunarThemeTextures.MSGlowOrb;
            if (msGlowOrb != null)
            {
                float orbPulse = 0.85f + 0.15f * (float)Math.Sin(Progression * MathHelper.Pi * 3f);
                Color orbColor = tidalOuter with { A = 0 };
                Main.spriteBatch.Draw(msGlowOrb, tipPos, null, orbColor * crescentOpacity * 0.2f * orbPulse,
                    0f, msGlowOrb.Size() / 2f, crescentScale * 0.35f * Projectile.scale * orbPulse, SpriteEffects.None, 0f);
            }
        }

        #endregion

        #region On Hit

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ItemLoader.OnHitNPC(Owner.HeldItem, Owner, target, hit, damageDone);
            NPCLoader.OnHitByItem(target, Owner, Owner.HeldItem, hit, damageDone);
            PlayerLoader.OnHitNPC(Owner, target, hit, damageDone);

            // Apply tidal drowning debuff
            target.AddBuff(ModContent.BuffType<TidalDrowning>(), 180);

            // === GRAVITATIONAL PULL ON HIT ===
            // Hits apply a weak vortex — enemies near the target are slowly pulled toward impact for 1 second
            Owner.EternalMoon().StartGravitationalPull(target.Center);

            // Gravity well VFX
            if (!Main.dedServ)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 spawnPos = target.Center + angle.ToRotationVector2() * Main.rand.NextFloat(60f, 120f);
                    LunarParticleHandler.SpawnParticle(new GravityWellMoteParticle(
                        spawnPos, target.Center, Main.rand.NextFloat(0.3f, 0.5f),
                        Color.Lerp(Violet, IceBlue, Main.rand.NextFloat()), Main.rand.Next(30, 50)));
                }
            }

            // === FOUNDATION VFX: Impact Effects on Regular Swings ===
            if (Main.myPlayer == Projectile.owner && State == SwingState.Swinging)
            {
                // TidalThinSlash — razor-thin slash mark at hit direction angle
                float hitAngle = (target.Center - Owner.MountedCenter).ToRotation();
                int slashStyle = _lunarPhase >= 2 ? 1 : 0; // Violet Cut at higher phases, Ice Cyan at lower
                Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                    target.Center, Vector2.Zero,
                    ModContent.ProjectileType<TidalThinSlash>(),
                    0, 0f, Projectile.owner, hitAngle, slashStyle);

                // Phase 2+ spawns TidalRippleEffect — expanding concentric rings
                if (_lunarPhase >= 2)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                        target.Center, Vector2.Zero,
                        ModContent.ProjectileType<TidalRippleEffect>(),
                        0, 0f, Projectile.owner, _phaseIntensity);
                }
            }

            // === LUNAR SURGE HIT ===
            if (State == SwingState.LunarSurge)
            {
                Owner.itemAnimation = 0;
                Owner.velocity = Owner.SafeDirectionTo(target.Center) * -ReboundSpeed;
                Projectile.timeLeft = FullMoonOpportunity + SurgeCooldown;
                InPostSurgeStasis = true;
                Projectile.netUpdate = true;

                SoundEngine.PlaySound(SoundID.Item125 with { Volume = 0.8f }, target.Center);

                // Apply lunar stasis freeze
                target.AddBuff(ModContent.BuffType<LunarStasis>(), 90);

                // Spawn crescent slash VFX at target
                if (Main.myPlayer == Projectile.owner)
                {
                    int slashDamage = (int)(Projectile.damage * 0.6f);
                    for (int i = 0; i < 3; i++)
                    {
                        int proj = Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                            target.Center, Projectile.velocity * 0.05f,
                            ModContent.ProjectileType<EternalMoonCrescentSlash>(),
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
                        LunarParticleHandler.SpawnParticle(new CrescentSparkParticle(
                            target.Center, sparkVel, Main.rand.NextFloat(0.5f, 1f), IceBlue, 20));
                    }

                    LunarParticleHandler.SpawnParticle(new LunarBloomParticle(
                        target.Center, 1f, CrescentGlow, 25, 0.06f));
                }

                // Foundation VFX: Cross-slash thin lines on Surge impact
                if (Main.myPlayer == Projectile.owner)
                {
                    float surgeAngle = Projectile.velocity.ToRotation();
                    for (int i = 0; i < 2; i++)
                    {
                        float slashAngle = surgeAngle + MathHelper.PiOver4 * (i == 0 ? 1 : -1);
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                            target.Center, Vector2.Zero,
                            ModContent.ProjectileType<TidalThinSlash>(),
                            0, 0f, Projectile.owner, slashAngle, 1); // Style 1: Violet Cut (Surge intensity)
                    }

                    // Ripple expanding rings at surge impact
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                        target.Center, Vector2.Zero,
                        ModContent.ProjectileType<TidalRippleEffect>(),
                        0, 0f, Projectile.owner, 1.2f); // Enhanced tidal strength for Surge
                }
            }

            // === FULL MOON EMPOWERED SLASH HIT ===
            if (State == SwingState.Swinging && PerformingFullMoonSlash &&
                Owner.ownedProjectileCounts[ModContent.ProjectileType<EternalMoonTidalDetonation>()] < 1)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f }, Projectile.Center);

                if (Main.myPlayer == Projectile.owner)
                {
                    int detonationDamage = (int)(Projectile.damage * DetonationDamageFactor);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                        target.Center, Vector2.Zero,
                        ModContent.ProjectileType<EternalMoonTidalDetonation>(),
                        detonationDamage, 0f, Projectile.owner);
                }

                // Lifesteal on Full Moon hit
                Owner.DoLifestealDirect(target, (int)Math.Round(hit.Damage * 0.05));

                // Massive impact VFX
                if (!Main.dedServ)
                {
                    // Bloom burst — layered from wide dark halo to white-hot core (reduced intensity)
                    LunarParticleHandler.SpawnParticle(new LunarBloomParticle(
                        target.Center, 0.8f, MoonWhite, 25, 0.06f));
                    LunarParticleHandler.SpawnParticle(new LunarBloomParticle(
                        target.Center, 1.2f, IceBlue, 30, 0.05f));
                    LunarParticleHandler.SpawnParticle(new LunarBloomParticle(
                        target.Center, 1.6f, DarkPurple, 40, 0.035f));

                    // Crescent spark explosion
                    for (int i = 0; i < 16; i++)
                    {
                        Vector2 sparkVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(6f, 14f);
                        LunarParticleHandler.SpawnParticle(new CrescentSparkParticle(
                            target.Center, sparkVel, Main.rand.NextFloat(0.6f, 1.2f),
                            CrescentGlow, Main.rand.Next(18, 30)));
                    }

                    // Hue-shifting music note cascade — the Full Moon's climactic chord
                    MoonlightVFXLibrary.SpawnMusicNotes(target.Center, count: 8, spread: 40f,
                        minScale: 0.8f, maxScale: 1.2f, lifetime: 60);

                    // Per-weapon lunar note cascade (complementary to the hue-shifting notes)
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 noteVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f));
                        LunarParticleHandler.SpawnParticle(new LunarNoteParticle(
                            target.Center + Main.rand.NextVector2Circular(30f, 30f),
                            noteVel, Main.rand.NextFloat(0.4f, 0.8f), Main.rand.Next(50, 80)));
                    }

                    // Radial dust burst from shared VFX library — concentric moonlight rings
                    MoonlightVFXLibrary.SpawnRadialDustBurst(target.Center, 16, 8f);

                    // Heavy tidal smoke
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 smokeVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1f, 3f);
                        LunarParticleHandler.SpawnParticle(new TidalSmokeParticle(
                            target.Center + Main.rand.NextVector2Circular(40f, 40f),
                            smokeVel, Main.rand.NextFloat(0.3f, 0.6f),
                            Color.Lerp(DarkPurple, NightPurple, Main.rand.NextFloat()),
                            Main.rand.Next(60, 100)));
                    }
                }
            }
        }

        #endregion

        #region On Kill

        public override void OnKill(int timeLeft)
        {
            Owner.fullRotation = 0f;
            Owner.EternalMoon().SurgingForward = false;
        }

        #endregion
    }
}
