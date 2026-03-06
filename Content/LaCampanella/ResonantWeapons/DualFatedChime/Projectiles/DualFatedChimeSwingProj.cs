using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Primitives;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Shaders;
using MagnumOpus.Content.LaCampanella;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using MagnumOpus.Content.FoundationWeapons.ExplosionParticlesFoundation;
using MagnumOpus.Content.FoundationWeapons.ThinSlashFoundation;
using MagnumOpus.Content.FoundationWeapons.XSlashFoundation;
using MagnumOpus.Content.FoundationWeapons.RibbonFoundation;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Projectiles
{
    /// <summary>
    /// Dual Fated Chime — 5-phase alternating inferno waltz combo.
    /// 
    /// Phase 0 "Opening Peal":  Right chime horizontal slash + bell shockwave ring
    /// Phase 1 "Answer":        Left chime diagonal slash — faster, reversed
    /// Phase 2 "Escalation":    Right chime upward arc + flame wave projectile
    /// Phase 3 "Resonance":     Left chime downward slam + double shockwave + ground fire
    /// Phase 4 "Grand Toll":    Both chimes cross-slash + 12 directional Bell Flame Waves
    /// 
    /// Bell Resonance Stacking: Each hit adds a Resonance Ring to hit enemy (max 5).
    /// At 5 rings, next hit triggers Bell Shatter — massive AoE damage burst.
    /// </summary>
    public class DualFatedChimeSwingProj : ModProjectile
    {
        #region Constants

        private static readonly int[] BladeLengths = { 155, 155, 160, 165, 180 };
        private static readonly int[] Durations = { 16, 14, 20, 22, 28 };
        private static readonly float[] DamageMults = { 0.85f, 0.90f, 1.0f, 1.15f, 1.50f };
        private static readonly float[] MaxAngles =
        {
            MathHelper.PiOver2 * 1.3f,
            MathHelper.PiOver2 * 1.4f,
            MathHelper.PiOver2 * 1.6f,
            MathHelper.PiOver2 * 1.8f,
            MathHelper.PiOver2 * 2.2f
        };

        private const int TrailPointCount = 40;
        private const int RenderPointCount = 80;

        #endregion

        #region Curve Definitions

        // Phase 0: Opening Peal — quick horizontal bell ring
        private static readonly DualFatedChimeUtils.CurveSegment[] OpeningPealAnim = new[]
        {
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.SineOutEasing, 0f, -0.85f, 0.2f, 2),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyInEasing, 0.12f, -0.65f, 1.60f, 3),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.SineOutEasing, 0.78f, 0.95f, 0.05f, 2),
        };

        // Phase 1: Answer — faster reversed diagonal
        private static readonly DualFatedChimeUtils.CurveSegment[] AnswerAnim = new[]
        {
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.SineOutEasing, 0f, -0.90f, 0.15f, 2),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyInEasing, 0.10f, -0.75f, 1.70f, 4),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.SineOutEasing, 0.75f, 0.95f, 0.05f, 2),
        };

        // Phase 2: Escalation — upward arc with extra lift
        private static readonly DualFatedChimeUtils.CurveSegment[] EscalationAnim = new[]
        {
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyOutEasing, 0f, -1.0f, 0.30f, 2),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyInEasing, 0.18f, -0.70f, 1.65f, 3),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyOutEasing, 0.80f, 0.95f, 0.05f, 2),
        };

        // Phase 3: Resonance — dramatic wind-back + slam down
        private static readonly DualFatedChimeUtils.CurveSegment[] ResonanceAnim = new[]
        {
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.SineBumpEasing, 0f, -1.0f, -0.15f, 2),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyOutEasing, 0.12f, -1.15f, 0.45f, 2),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyInEasing, 0.30f, -0.70f, 1.75f, 5),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyOutEasing, 0.82f, 1.05f, -0.05f, 2),
        };

        // Phase 4: Grand Toll — massive wind-back + explosive cross-slash
        private static readonly DualFatedChimeUtils.CurveSegment[] GrandTollAnim = new[]
        {
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.SineBumpEasing, 0f, -1.1f, -0.20f, 2),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyOutEasing, 0.08f, -1.30f, 0.50f, 2),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyInEasing, 0.28f, -0.80f, 1.90f, 5),
            new DualFatedChimeUtils.CurveSegment(DualFatedChimeUtils.PolyOutEasing, 0.80f, 1.10f, -0.10f, 2),
        };

        private static readonly DualFatedChimeUtils.CurveSegment[][] AllAnimations =
        {
            OpeningPealAnim, AnswerAnim, EscalationAnim, ResonanceAnim, GrandTollAnim
        };

        #endregion

        #region State Properties

        public int ComboPhase => Math.Clamp((int)Projectile.ai[0], 0, 4);
        public float Progression => Math.Clamp((float)Timer / SwingDuration, 0f, 1f);
        public int Timer => SwingDuration - Projectile.timeLeft;
        public int SwingDuration => Durations[ComboPhase];
        public float BladeLength => BladeLengths[ComboPhase];
        public float MaxAngle => MaxAngles[ComboPhase];
        public DualFatedChimeUtils.CurveSegment[] CurrentAnimation => AllAnimations[ComboPhase];
        public float DamageMultiplier => DamageMults[ComboPhase];

        // Phases 1 and 3 are left-chime (flipped)
        public bool IsFlipped => ComboPhase == 1 || ComboPhase == 3;

        public int Direction { get; private set; }
        public float BaseRotation { get; private set; }

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private float _squishFactor;
        private bool _flamesSpawned;
        private bool _groundFireSpawned;
        private bool _soundPlayed;

        private Vector2[] _trailPositions;
        private DualFatedChimePrimitiveRenderer _trailRenderer;

        #endregion

        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/DualFatedChime/DualFatedChime";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.ownerHitCheck = true;
        }

        public override void AI()
        {
            if (!_initialized)
                Initialize();

            Projectile.Center = Owner.MountedCenter;
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            float animValue = DualFatedChimeUtils.PiecewiseAnimation(Progression, CurrentAnimation);
            int flipSign = IsFlipped ? -1 : 1;
            float swingAngle = animValue * MaxAngle * Direction * flipSign;
            float currentRotation = BaseRotation + swingAngle;

            Projectile.rotation = currentRotation;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, currentRotation - MathHelper.PiOver2);

            DoSwingVFX(currentRotation);

            // Phase 2 (Escalation): Fire flame wave projectile at 70%
            if (ComboPhase == 2 && Progression >= 0.70f && !_flamesSpawned)
            {
                SpawnFlameWaveProjectile(currentRotation);
                _flamesSpawned = true;
            }

            // Phase 3 (Resonance): Double shockwave + ground fire at 65%
            if (ComboPhase == 3 && Progression >= 0.65f && !_groundFireSpawned)
            {
                SpawnResonanceGroundFire(currentRotation);
                _groundFireSpawned = true;
            }

            // Phase 4 (Grand Toll): 12 directional flame waves at 65%
            if (ComboPhase == 4 && Progression >= 0.65f && !_flamesSpawned)
            {
                SpawnGrandTollFlameCircle(currentRotation);
                _flamesSpawned = true;
            }

            // Play sound at 20% progress — pitch rises with combo phase
            if (Progression >= 0.20f && !_soundPlayed)
            {
                float pitch = -0.15f + ComboPhase * 0.08f;
                float volume = 0.6f + ComboPhase * 0.05f;
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = pitch, Volume = volume }, Projectile.Center);
                _soundPlayed = true;
            }

            // Shrink toward end
            if (Progression > 0.75f)
            {
                float shrinkProgress = (Progression - 0.75f) / 0.25f;
                Projectile.scale = MathHelper.Lerp(1f, 0.3f, shrinkProgress);
            }
            else
            {
                Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f, 0.15f);
            }

            // Infernal lighting along blade
            Vector2 swordDir = currentRotation.ToRotationVector2();
            float phaseIntensity = 0.4f + ComboPhase * 0.1f;
            for (int i = 0; i < 5; i++)
            {
                float bladeT = (float)i / 4f;
                Vector2 lightPos = Projectile.Center + swordDir * BladeLength * bladeT * Projectile.scale;
                float intensity = phaseIntensity * (1f - bladeT * 0.3f);
                Vector3 fireLight = new Vector3(0.8f, 0.35f + bladeT * 0.3f, 0.05f);
                Lighting.AddLight(lightPos, fireLight * intensity);
            }

            if (Projectile.timeLeft <= 0)
                Projectile.Kill();
        }

        private void Initialize()
        {
            _initialized = true;
            Direction = Math.Sign(Projectile.velocity.X) != 0 ? Math.Sign(Projectile.velocity.X) : 1;
            BaseRotation = Projectile.velocity.ToRotation();
            _squishFactor = MathHelper.Lerp(0.90f, 0.80f, ComboPhase / 4f);
            Projectile.timeLeft = SwingDuration;
            _trailPositions = new Vector2[TrailPointCount];
            _trailRenderer = new DualFatedChimePrimitiveRenderer();

            Projectile.damage = (int)(Projectile.damage * DamageMultiplier);
            Owner.direction = Direction;
        }

        #region Swing VFX

        private void DoSwingVFX(float currentRotation)
        {
            Vector2 swordDir = currentRotation.ToRotationVector2();
            Vector2 tipPos = Projectile.Center + swordDir * BladeLength * Projectile.scale;

            // Infernal embers flying off blade during active swing — intensity scales with phase
            if (Progression > 0.15f && Progression < 0.90f)
            {
                float dustChance = MathHelper.Lerp(0.3f, 1f, Progression) * (1f + ComboPhase * 0.15f);
                if (Main.rand.NextFloat() < dustChance)
                {
                    float bladeT = Main.rand.NextFloat(0.3f, 1f);
                    Vector2 dustPos = Projectile.Center + swordDir * BladeLength * bladeT * Projectile.scale;
                    Vector2 perpVel = new Vector2(-swordDir.Y, swordDir.X) * Main.rand.NextFloat(1.5f, 4f) * Direction;

                    Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, perpVel, 0,
                        DualFatedChimeUtils.GetFireFlicker(bladeT), 1.1f + ComboPhase * 0.1f);
                    d.noGravity = true;
                    d.fadeIn = 0.9f;

                    if (Main.rand.NextBool(3))
                    {
                        Dust s = Dust.NewDustPerfect(dustPos, DustID.Smoke, perpVel * 0.4f, 80,
                            new Color(20, 15, 20), 1.5f);
                        s.noGravity = true;
                    }
                }
            }

            // Phase-specific midpoint VFX bursts
            float midpoint = 0.55f;
            if (Math.Abs(Progression - midpoint) < 0.03f)
            {
                int burstCount = 3 + ComboPhase * 2;
                for (int i = 0; i < burstCount; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(3f + ComboPhase * 0.5f, 3f + ComboPhase * 0.5f) + swordDir * 2f;
                    float heat = Main.rand.NextFloat(0.3f, 0.9f);
                    DualFatedChimeParticleHandler.SpawnParticle(
                        new InfernalEmberParticle(tipPos + Main.rand.NextVector2Circular(8f, 8f),
                            vel, heat, 18 + ComboPhase * 2, 0.4f + ComboPhase * 0.05f));
                }

                // Smoke for phases 2+
                if (ComboPhase >= 2)
                {
                    for (int i = 0; i < ComboPhase; i++)
                    {
                        Vector2 smokeVel = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0, -1f);
                        DualFatedChimeParticleHandler.SpawnParticle(
                            new BellSmokeParticle(tipPos + Main.rand.NextVector2Circular(15f, 15f),
                                smokeVel, 40, 1.0f + ComboPhase * 0.15f, 0.6f));
                    }
                }

                // Music note sparks at phases 3+
                if (ComboPhase >= 3)
                {
                    for (int i = 0; i < ComboPhase - 1; i++)
                    {
                        Vector2 noteVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -2f);
                        DualFatedChimeParticleHandler.SpawnParticle(
                            new MusicalFlameParticle(tipPos + Main.rand.NextVector2Circular(10f, 10f),
                                noteVel, 35, 0.5f + ComboPhase * 0.05f));
                    }
                }
            }

            GenerateTrailPoints();
        }

        private void GenerateTrailPoints()
        {
            float trailLength = Math.Min(Progression, 0.5f);
            float startProgress = Math.Max(0f, Progression - trailLength);

            for (int i = 0; i < TrailPointCount; i++)
            {
                float t = MathHelper.Lerp(startProgress, Progression, (float)i / (TrailPointCount - 1));
                float animValue = DualFatedChimeUtils.PiecewiseAnimation(t, CurrentAnimation);
                int flipSign = IsFlipped ? -1 : 1;
                float angle = BaseRotation + animValue * MaxAngle * Direction * flipSign;
                Vector2 dir = angle.ToRotationVector2();
                _trailPositions[i] = Projectile.Center + dir * BladeLength * Projectile.scale;
            }
        }

        #endregion

        #region Phase-Specific Projectile Spawning

        /// <summary>Phase 2: Single flame wave projectile from blade tip + Foundation RippleEffect.</summary>
        private void SpawnFlameWaveProjectile(float rotation)
        {
            Vector2 swordDir = rotation.ToRotationVector2();
            Vector2 tipPos = Projectile.Center + swordDir * BladeLength * Projectile.scale;
            Vector2 flameVel = swordDir * 10f + Main.rand.NextVector2Circular(0.5f, 0.5f);

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), tipPos, flameVel,
                ModContent.ProjectileType<BellFlameWaveProj>(),
                Projectile.damage / 3, Projectile.knockBack * 0.3f, Projectile.owner);

            // === FOUNDATION: RippleEffectProjectile — Bell ring wave at blade tip ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), tipPos, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner, ai0: 1f);

            DualFatedChimeParticleHandler.SpawnParticle(
                new InfernalEmberParticle(tipPos, flameVel * 0.3f, 0.7f, 18, 0.5f));
            DualFatedChimeParticleHandler.SpawnParticle(
                new BellChimeFlashParticle(tipPos, 15, 1.5f));

            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.1f, Volume = 0.5f }, tipPos);
        }

        /// <summary>Phase 3: Double shockwave ring + ground fire zone via Foundation DamageZone.</summary>
        private void SpawnResonanceGroundFire(float rotation)
        {
            Vector2 swordDir = rotation.ToRotationVector2();
            Vector2 tipPos = Projectile.Center + swordDir * BladeLength * Projectile.scale;

            // === FOUNDATION: RippleEffectProjectile — Double bell ring shockwave ===
            for (int ring = 0; ring < 2; ring++)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), tipPos, Vector2.Zero,
                    ModContent.ProjectileType<RippleEffectProjectile>(),
                    0, 0f, Projectile.owner, ai0: 1f);
            }

            // === FOUNDATION: DamageZoneProjectile — Persistent flame zone ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), tipPos, Vector2.Zero,
                ModContent.ProjectileType<DamageZoneProjectile>(),
                Projectile.damage / 3, 0f, Projectile.owner);

            // Expanding dust rings for immediate visual impact
            for (int ring = 0; ring < 2; ring++)
            {
                int dustCount = 16 + ring * 8;
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount;
                    Vector2 dustVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (3f + ring * 2f);
                    Dust d = Dust.NewDustPerfect(tipPos + dustVel * 2f, DustID.Torch, dustVel, 0,
                        DualFatedChimeUtils.GetInfernalGradient(0.5f + ring * 0.2f), 1.5f - ring * 0.3f);
                    d.noGravity = true;
                    d.fadeIn = 1.0f;
                }
            }

            // Ground fire lingering dust
            for (int i = 0; i < 10; i++)
            {
                Vector2 groundPos = tipPos + new Vector2(Main.rand.NextFloat(-60f, 60f), Main.rand.NextFloat(0f, 20f));
                Vector2 upVel = new Vector2(0, -Main.rand.NextFloat(0.5f, 2f));
                Dust d = Dust.NewDustPerfect(groundPos, DustID.Torch, upVel, 0,
                    DualFatedChimeUtils.GetFireFlicker(Main.rand.NextFloat()), 1.2f);
                d.noGravity = true;
            }

            // Flash + smoke burst
            DualFatedChimeParticleHandler.SpawnParticle(new BellChimeFlashParticle(tipPos, 20, 2.2f));
            for (int i = 0; i < 6; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(4f, 4f) + new Vector2(0, -0.5f);
                DualFatedChimeParticleHandler.SpawnParticle(
                    new BellSmokeParticle(tipPos + Main.rand.NextVector2Circular(20f, 20f),
                        smokeVel, 50, 1.5f, 0.7f));
            }

            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 0.7f }, tipPos);
        }

        /// <summary>Phase 4: Grand Toll — 12 directional Bell Flame Waves + Foundation XSlash + ExplosionParticles.</summary>
        private void SpawnGrandTollFlameCircle(float rotation)
        {
            Vector2 swordDir = rotation.ToRotationVector2();
            Vector2 tipPos = Projectile.Center + swordDir * BladeLength * Projectile.scale;
            int waveCount = 12;

            // === FOUNDATION: XSlashEffect — Cross-detonation at Grand Toll center ===
            // fireIntensity = 0.15 (very intense), InfernalOrange → BellGold → WhiteHot
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), tipPos, Vector2.Zero,
                ModContent.ProjectileType<XSlashEffect>(),
                0, 0f, Projectile.owner,
                ai0: rotation, ai1: (float)XSlashStyle.LaCampanella);

            // === FOUNDATION: SparkExplosionProjectile — Bell Shatter spark burst (60 sparks) ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), tipPos, Vector2.Zero,
                ModContent.ProjectileType<SparkExplosionProjectile>(),
                (int)(Projectile.damage * 0.3f), Projectile.knockBack * 0.3f, Projectile.owner,
                ai0: (float)SparkMode.RadialScatter);

            // 12 directional Bell Flame Waves
            for (int i = 0; i < waveCount; i++)
            {
                float angle = MathHelper.TwoPi * i / waveCount;
                Vector2 flameDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Vector2 flameVel = flameDir * 9f;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), tipPos, flameVel,
                    ModContent.ProjectileType<BellFlameWaveProj>(),
                    (int)(Projectile.damage * 0.4f), Projectile.knockBack * 0.2f, Projectile.owner);

                // Ember trail at each wave spawn point
                DualFatedChimeParticleHandler.SpawnParticle(
                    new InfernalEmberParticle(tipPos, flameVel * 0.2f, 0.8f, 15, 0.4f));
            }

            // === FOUNDATION: RippleEffectProjectile — Massive bell shockwave ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), tipPos, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner, ai0: 1f);

            // Massive bell chime flash
            DualFatedChimeParticleHandler.SpawnParticle(new BellChimeFlashParticle(tipPos, 25, 3.0f));

            // Heavy smoke ring
            for (int i = 0; i < 12; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                DualFatedChimeParticleHandler.SpawnParticle(
                    new BellSmokeParticle(tipPos, smokeVel, 55, 2.0f, 0.8f));
            }

            // Music note burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2CircularEdge(3f, 3f) + new Vector2(0, -2f);
                DualFatedChimeParticleHandler.SpawnParticle(
                    new MusicalFlameParticle(tipPos, noteVel, 45, 0.7f));
            }

            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.3f, Volume = 0.9f }, tipPos);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.1f, Volume = 0.6f }, tipPos);
        }

        #endregion

        #region Collision

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float currentAngle = Projectile.rotation;
            Vector2 swordDir = currentAngle.ToRotationVector2();
            Vector2 start = Projectile.Center;
            Vector2 end = start + swordDir * (BladeLength + 40f) * Projectile.scale;
            float width = 30f;
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref _);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            int expand = 20;
            hitbox.Inflate(expand, expand);
        }

        #endregion

        #region On Hit — Bell Resonance Stacking + Foundation Impacts

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Resonant Toll (existing debuff system)
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);

            // Bell Resonance Stacking: track per-NPC resonance rings
            var resonance = target.GetGlobalNPC<BellResonanceNPC>();
            resonance.AddResonanceRing(target, Projectile.owner);

            Vector2 hitPos = target.Center;

            // === FOUNDATION: RippleEffectProjectile — Bell ring shockwave on every hit ===
            // Bell chime visual: concentric ripple rings expanding from impact
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), hitPos, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner, ai0: 1f);

            // === FOUNDATION: ThinSlashEffect — Thin flame slash marks on hit (Phase 2+) ===
            if (ComboPhase >= 2)
            {
                float slashAngle = Projectile.rotation + Main.rand.NextFloat(-0.2f, 0.2f);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), hitPos, Vector2.Zero,
                    ModContent.ProjectileType<ThinSlashEffect>(),
                    0, 0f, Projectile.owner,
                    ai0: slashAngle, ai1: (float)SlashStyle.GoldenEdge);
            }

            // Custom particle impact VFX — scales with combo phase
            int emberCount = 5 + ComboPhase * 2;
            for (int i = 0; i < emberCount; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f + ComboPhase, 4f + ComboPhase);
                float heat = Main.rand.NextFloat(0.4f, 0.9f);
                DualFatedChimeParticleHandler.SpawnParticle(
                    new InfernalEmberParticle(hitPos, sparkVel, heat, 20 + ComboPhase * 2, 0.4f + ComboPhase * 0.05f));
            }

            // Smoke puff
            for (int i = 0; i < 2 + ComboPhase; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(2f, 2f);
                DualFatedChimeParticleHandler.SpawnParticle(
                    new BellSmokeParticle(hitPos + Main.rand.NextVector2Circular(10f, 10f),
                        smokeVel, 35, 1f, 0.5f));
            }

            // Bell chime flash on crit or higher phases
            if (hit.Crit || ComboPhase >= 3)
            {
                DualFatedChimeParticleHandler.SpawnParticle(
                    new BellChimeFlashParticle(hitPos, 18, 1.5f + ComboPhase * 0.2f));

                for (int i = 0; i < 2; i++)
                {
                    Vector2 noteVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -2f);
                    DualFatedChimeParticleHandler.SpawnParticle(
                        new MusicalFlameParticle(hitPos, noteVel, 30, 0.5f));
                }
            }
        }

        #endregion

        #region Rendering

        private static bool _particlesDrawnThisFrame;
        private static int _lastParticleDrawFrame;

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            try
            {
                // Layer 1: Shader-driven infernal swing trail
                if (Progression > 0.18f)
                    DrawSlashTrail(sb);

                // Layer 2: Bloom underlays at blade tip
                DrawBloomUnderlays(sb);

                // Layer 3: Blade sprite
                DrawBlade(sb, lightColor);

                // Layer 4: Particles — only draw once per frame across all instances
                int currentFrame = (int)Main.GameUpdateCount;
                if (_lastParticleDrawFrame != currentFrame)
                {
                    _lastParticleDrawFrame = currentFrame;
                    DualFatedChimeParticleHandler.DrawAllParticles(sb);
                }

                // Theme texture accents
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                DualFatedChimeUtils.DrawThemeAccents(sb, Projectile.Center - Main.screenPosition, Projectile.scale);
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch
            {
                // Ensure SpriteBatch is restored on any rendering failure
                try
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }

            return false;
        }

        private void DrawSlashTrail(SpriteBatch sb)
        {
            if (_trailPositions == null || _trailRenderer == null)
                return;

            Color primaryColor = DualFatedChimeUtils.GetInfernalGradient(0.3f + ComboPhase * 0.12f);
            Color secondaryColor = DualFatedChimeUtils.GetInfernalGradient(0.5f + ComboPhase * 0.10f);
            Color edgeColor = DualFatedChimeUtils.GetInfernalGradient(0.75f + ComboPhase * 0.05f);

            float trailOpacity = MathHelper.Clamp((Progression - 0.18f) / 0.15f, 0f, 1f);
            if (Progression > 0.85f)
                trailOpacity *= 1f - (Progression - 0.85f) / 0.15f;

            MiscShaderData shader = DualFatedChimeShaderLoader.GetSlashShader();

            if (shader != null)
            {
                shader.UseColor(primaryColor);
                shader.UseSecondaryColor(secondaryColor);

                try
                {
                    shader.Shader.Parameters["fireColor"]?.SetValue(edgeColor.ToVector4());
                    shader.Shader.Parameters["uTime"]?.SetValue(Main.GameUpdateCount * 0.025f);
                    shader.Shader.Parameters["flipped"]?.SetValue(IsFlipped);
                }
                catch { }

                try
                {
                    var noiseTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileableFBMNoise",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad);
                    if (noiseTexture?.Value != null)
                    {
                        Main.graphics.GraphicsDevice.Textures[1] = noiseTexture.Value;
                        Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
                    }
                }
                catch { }
            }

            var mainSettings = new DualFatedChimeTrailSettings(
                width: (float t) =>
                {
                    float baseWidth = (1f - t * 0.6f) * BladeLength * 0.5f * Projectile.scale * _squishFactor;
                    return baseWidth * trailOpacity;
                },
                trailColor: (float t) =>
                {
                    Color col = Color.Lerp(primaryColor, secondaryColor, t);
                    return col * trailOpacity * (1f - t * 0.3f);
                },
                shader: shader,
                smoothen: true
            );

            try
            {
                sb.End();
            }
            catch { }

            try
            {
                _trailRenderer.RenderTrail(_trailPositions, mainSettings, RenderPointCount);

                if (shader != null)
                {
                    try
                    {
                        shader.UseColor(edgeColor * 0.6f);
                        shader.UseSecondaryColor(new Color(255, 240, 200) * 0.3f);
                    }
                    catch { }
                }

                var glowSettings = new DualFatedChimeTrailSettings(
                    width: (float t) =>
                    {
                        float baseWidth = (1f - t * 0.5f) * BladeLength * 0.6f * Projectile.scale * _squishFactor;
                        return baseWidth * trailOpacity * 0.5f;
                    },
                    trailColor: (float t) =>
                    {
                        return DualFatedChimeUtils.Additive(edgeColor, trailOpacity * 0.35f * (1f - t));
                    },
                    shader: shader,
                    smoothen: true
                );

                _trailRenderer.RenderTrail(_trailPositions, glowSettings, RenderPointCount);
            }
            finally
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        private void DrawBloomUnderlays(SpriteBatch sb)
        {
            float currentAngle = Projectile.rotation;
            Vector2 swordDir = currentAngle.ToRotationVector2();
            Vector2 tipPos = Projectile.Center + swordDir * BladeLength * Projectile.scale - Main.screenPosition;

            Texture2D bloomTex = null;
            try
            {
                bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }
            if (bloomTex == null) return;

            Vector2 bloomOrigin = new Vector2(bloomTex.Width / 2f, bloomTex.Height / 2f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float bloomPulse = 0.8f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.15f);
            float phaseScale = 1f + ComboPhase * 0.1f;

            // Outer flame glow — orange
            Color outerBloom = DualFatedChimeUtils.Additive(new Color(255, 100, 0), 0.2f * bloomPulse);
            sb.Draw(bloomTex, tipPos, null, outerBloom, 0f, bloomOrigin, 1.4f * Projectile.scale * phaseScale, SpriteEffects.None, 0f);

            // Mid bloom — gold
            Color midBloom = DualFatedChimeUtils.Additive(new Color(255, 200, 50), 0.3f * bloomPulse);
            sb.Draw(bloomTex, tipPos, null, midBloom, 0f, bloomOrigin, 0.7f * Projectile.scale * phaseScale, SpriteEffects.None, 0f);

            // White-hot core
            Color coreBloom = DualFatedChimeUtils.Additive(new Color(255, 240, 200), 0.5f * bloomPulse);
            sb.Draw(bloomTex, tipPos, null, coreBloom, 0f, bloomOrigin, 0.3f * Projectile.scale * phaseScale, SpriteEffects.None, 0f);

            // Extra bloom for phases 3 and 4
            if (ComboPhase >= 3)
            {
                Color phaseBloom = DualFatedChimeUtils.Additive(new Color(255, 60, 0), 0.12f * bloomPulse * (ComboPhase - 2));
                sb.Draw(bloomTex, tipPos, null, phaseBloom, 0f, bloomOrigin, 2.0f * Projectile.scale * phaseScale, SpriteEffects.None, 0f);
            }

            // --- LC Radial Slash Star Impact — sharp infernal star flare on blade tip ---
            LaCampanellaVFXLibrary.DrawRadialSlashStar(sb, tipPos,
                0.25f * Projectile.scale * phaseScale,
                (float)Main.GameUpdateCount * 0.04f,
                0.3f * bloomPulse,
                LaCampanellaPalette.FlameYellow);

            // --- LC Power Effect Ring — concentric ring on higher combo phases ---
            if (ComboPhase >= 2)
            {
                float ringPulse = 0.7f + 0.3f * (float)Math.Sin(Main.GameUpdateCount * 0.12f);
                LaCampanellaVFXLibrary.DrawPowerEffectRing(sb, tipPos,
                    0.3f * Projectile.scale * phaseScale,
                    -(float)Main.GameUpdateCount * 0.025f,
                    0.2f * ringPulse * (ComboPhase - 1),
                    LaCampanellaPalette.InfernalOrange);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawBlade(SpriteBatch sb, Color lightColor)
        {
            Texture2D bladeTex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(bladeTex.Width * 0.5f, bladeTex.Height);

            float rot = Projectile.rotation + MathHelper.PiOver4;
            SpriteEffects effects = Direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float squish = MathHelper.Lerp(_squishFactor, 1f, Progression);
            Vector2 squishScale = new Vector2(1f + (1f - squish) * 0.5f, squish) * Projectile.scale;

            // Shadow
            sb.Draw(bladeTex, drawPos + new Vector2(-1, 1), null,
                new Color(0, 0, 0, 100) * 0.3f, rot, origin, squishScale * 1.02f, effects, 0f);

            // Main blade
            sb.Draw(bladeTex, drawPos, null, lightColor, rot, origin, squishScale, effects, 0f);

            // Infernal fire glow overlay — must use additive blend since Additive() sets A=0
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Color fireGlow = DualFatedChimeUtils.Additive(DualFatedChimeUtils.GetFireFlicker(), 0.15f + ComboPhase * 0.03f);
            sb.Draw(bladeTex, drawPos, null, fireGlow, rot, origin, squishScale * 1.01f, effects, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        public override void OnKill(int timeLeft)
        {
            _trailRenderer?.Dispose();
        }
    }
}
