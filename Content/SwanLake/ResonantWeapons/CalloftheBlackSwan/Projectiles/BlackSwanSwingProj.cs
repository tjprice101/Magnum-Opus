using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Utilities;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Projectiles
{
    /// <summary>
    /// Call of the Black Swan — Main held swing projectile.
    /// Exoblade-style state machine handling 3-phase ballet combo.
    /// Phase 0 "Entrechat": Quick diagonal slash, spawns feather projectiles
    /// Phase 1 "Fouetté": Spinning horizontal slash, spawns flare AoE
    /// Phase 2 "Grand Jeté": Overhead slam, swan shockwave + feather rain
    ///
    /// Foundation-pattern rendering: safe SpriteBatch management, bloom stacking,
    /// SpriteBatch-based trail, no custom particle handlers in PreDraw.
    /// Color identity: black/white gradient with rainbow accent highlights.
    /// </summary>
    public class BlackSwanSwingProj : ModProjectile
    {
        #region Constants

        private const int BladeLength_Entrechat = 155;
        private const int BladeLength_Fouette = 160;
        private const int BladeLength_GrandJete = 175;

        private const int Duration_Entrechat = 20;
        private const int Duration_Fouette = 24;
        private const int Duration_GrandJete = 28;

        private const float DamageMult_Entrechat = 0.85f;
        private const float DamageMult_Fouette = 1.0f;
        private const float DamageMult_GrandJete = 1.4f;

        private const float MaxSwingAngle_Entrechat = MathHelper.PiOver2 * 1.5f;
        private const float MaxSwingAngle_Fouette = MathHelper.PiOver2 * 1.6f;
        private const float MaxSwingAngle_GrandJete = MathHelper.PiOver2 * 1.8f;

        private const int TrailPointCount = 40;

        #endregion

        #region Curve Definitions

        private static readonly BlackSwanUtils.CurveSegment[] EntrechatAnimation = new[]
        {
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.SineOutEasing, 0f, -0.85f, 0.2f, 2),
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.PolyInEasing, 0.15f, -0.65f, 1.55f, 3),
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.SineOutEasing, 0.80f, 0.90f, 0.10f, 2),
        };

        private static readonly BlackSwanUtils.CurveSegment[] FouetteAnimation = new[]
        {
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.PolyOutEasing, 0f, -1.0f, 0.35f, 2),
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.PolyInEasing, 0.25f, -0.65f, 1.55f, 4),
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.PolyOutEasing, 0.82f, 0.90f, 0.10f, 2),
        };

        private static readonly BlackSwanUtils.CurveSegment[] GrandJeteAnimation = new[]
        {
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.SineBumpEasing, 0f, -1.0f, -0.15f, 2),
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.PolyOutEasing, 0.12f, -1.15f, 0.45f, 2),
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.PolyInEasing, 0.35f, -0.70f, 1.70f, 5),
            new BlackSwanUtils.CurveSegment(BlackSwanUtils.PolyOutEasing, 0.85f, 1.0f, 0.0f, 2),
        };

        #endregion

        #region State Properties

        public int ComboPhase => (int)Projectile.ai[0];

        public float Progression => Math.Clamp((float)Timer / SwingDuration, 0f, 1f);

        public int Timer => SwingDuration - Projectile.timeLeft;

        public int SwingDuration => ComboPhase switch
        {
            0 => Duration_Entrechat,
            1 => Duration_Fouette,
            _ => Duration_GrandJete
        };

        public float BladeLength => ComboPhase switch
        {
            0 => BladeLength_Entrechat,
            1 => BladeLength_Fouette,
            _ => BladeLength_GrandJete
        };

        public float MaxAngle => ComboPhase switch
        {
            0 => MaxSwingAngle_Entrechat,
            1 => MaxSwingAngle_Fouette,
            _ => MaxSwingAngle_GrandJete
        };

        public BlackSwanUtils.CurveSegment[] CurrentAnimation => ComboPhase switch
        {
            0 => EntrechatAnimation,
            1 => FouetteAnimation,
            _ => GrandJeteAnimation
        };

        public float DamageMultiplier => ComboPhase switch
        {
            0 => DamageMult_Entrechat,
            1 => DamageMult_Fouette,
            _ => DamageMult_GrandJete
        };

        public bool IsFlipped => ComboPhase == 1;

        public int Direction { get; private set; }
        public float BaseRotation { get; private set; }

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private float _squishFactor;
        private bool _flaresSpawned;
        private bool _soundPlayed;

        // Trail positions for rendering
        private Vector2[] _trailPositions;

        #endregion

        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/CalloftheBlackSwan/CalloftheBlackSwan";

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

        #region AI

        public override void AI()
        {
            if (!_initialized)
                Initialize();

            // Set swinging state for Dark Mirror conversion
            if (Owner != null && Owner.active)
            {
                try { Owner.GetModPlayer<BlackSwanPlayer>().IsSwinging = true; } catch { }
            }

            // Pin to player
            Projectile.Center = Owner.MountedCenter;
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            // Evaluate swing angle
            float animValue = BlackSwanUtils.PiecewiseAnimation(Progression, CurrentAnimation);
            int flipSign = IsFlipped ? -1 : 1;
            float swingAngle = animValue * MaxAngle * Direction * flipSign;
            float currentRotation = BaseRotation + swingAngle;

            Projectile.rotation = currentRotation;

            // Set player arm rotation
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, currentRotation - MathHelper.PiOver2);

            // Swing VFX (vanilla Dust only)
            DoSwingVFX(currentRotation);

            // Fire phase-specific sub-projectiles at 70% progress
            if (Progression >= 0.70f && !_flaresSpawned)
            {
                SpawnFlares(currentRotation);
                _flaresSpawned = true;
            }

            // Play sound at 20% progress
            if (Progression >= 0.20f && !_soundPlayed)
            {
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.1f + ComboPhase * 0.15f, Volume = 0.85f }, Projectile.Center);
                _soundPlayed = true;
            }

            // Scale management
            float targetScale = 1f;
            try
            {
                if (Owner.GetModPlayer<BlackSwanPlayer>().IsEmpowered && ComboPhase == 2)
                    targetScale = 1.5f;
            }
            catch { }

            if (Progression > 0.75f)
            {
                float shrinkProgress = (Progression - 0.75f) / 0.25f;
                Projectile.scale = MathHelper.Lerp(targetScale, 0.3f, shrinkProgress);
            }
            else
            {
                Projectile.scale = MathHelper.Lerp(Projectile.scale, targetScale, 0.15f);
            }

            // Lighting along blade
            Vector2 swordDir = currentRotation.ToRotationVector2();
            for (int i = 0; i < 5; i++)
            {
                float bladeT = (float)i / 4f;
                Vector2 lightPos = Projectile.Center + swordDir * BladeLength * bladeT * Projectile.scale;
                float intensity = 0.4f * (1f - bladeT * 0.5f);
                bool isBlack = (Timer + i) % 2 == 0;
                Vector3 lightColor = isBlack ? new Vector3(0.1f, 0.1f, 0.15f) : new Vector3(0.6f, 0.6f, 0.7f);
                Lighting.AddLight(lightPos, lightColor * intensity);
            }

            // Generate trail positions
            GenerateTrailPoints();

            if (Projectile.timeLeft <= 0)
                Projectile.Kill();
        }

        private void Initialize()
        {
            _initialized = true;
            Direction = Math.Sign(Projectile.velocity.X) != 0 ? Math.Sign(Projectile.velocity.X) : 1;
            BaseRotation = Projectile.velocity.ToRotation();
            _squishFactor = Main.rand.NextFloat(0.75f, 1f);
            Projectile.timeLeft = SwingDuration;
            _trailPositions = new Vector2[TrailPointCount];
            Projectile.damage = (int)(Projectile.damage * DamageMultiplier);
            Owner.direction = Direction;
        }

        #endregion

        #region Swing VFX (Vanilla Dust)

        private void DoSwingVFX(float currentRotation)
        {
            Vector2 swordDir = currentRotation.ToRotationVector2();
            Vector2 tipPos = Projectile.Center + swordDir * BladeLength * Projectile.scale;

            // Dual-polarity dust along blade
            if (Progression > 0.15f && Progression < 0.90f)
            {
                float dustChance = MathHelper.Lerp(0.3f, 0.9f, Progression);
                if (Main.rand.NextFloat() < dustChance)
                {
                    float bladeT = Main.rand.NextFloat(0.3f, 1f);
                    Vector2 dustPos = Projectile.Center + swordDir * BladeLength * bladeT * Projectile.scale;
                    Vector2 perpVel = new Vector2(-swordDir.Y, swordDir.X) * Main.rand.NextFloat(1f, 3f) * Direction;

                    bool isBlack = Main.rand.NextBool();
                    int dustType = isBlack ? DustID.Shadowflame : DustID.WhiteTorch;
                    Dust d = Dust.NewDustPerfect(dustPos, dustType, perpVel, 0,
                        isBlack ? new Color(30, 30, 40) : new Color(240, 240, 250), 1.2f);
                    d.noGravity = true;
                    d.fadeIn = 0.8f;
                }

                // Rainbow accent dust at blade tip
                if (Main.rand.NextBool(4))
                {
                    float hue = (Main.GameUpdateCount * 0.015f + Progression) % 1f;
                    Color rainbow = Main.hslToRgb(hue, 0.85f, 0.8f);
                    Dust rd = Dust.NewDustPerfect(tipPos + Main.rand.NextVector2Circular(6f, 6f),
                        DustID.RainbowTorch, Main.rand.NextVector2Circular(2f, 2f), 0, rainbow, 0.7f);
                    rd.noGravity = true;
                }
            }

            // Feather dust on Phase 1 midpoint
            if (ComboPhase == 1 && Math.Abs(Progression - 0.55f) < 0.03f)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + swordDir * 2f;
                    bool isBlack = Main.rand.NextBool();
                    Dust d = Dust.NewDustPerfect(tipPos + Main.rand.NextVector2Circular(10f, 10f),
                        isBlack ? DustID.Shadowflame : DustID.WhiteTorch, vel, 0,
                        isBlack ? new Color(30, 30, 40) : new Color(248, 245, 255), 1.4f);
                    d.noGravity = true;
                }
            }

            // Phase 2 empowered dual sparks
            bool isEmpowered = false;
            try { isEmpowered = Owner.GetModPlayer<BlackSwanPlayer>().IsEmpowered; } catch { }
            if (ComboPhase == 2 && isEmpowered && Progression > 0.3f && Progression < 0.7f)
            {
                if (Main.rand.NextBool(2))
                {
                    float bladeT = Main.rand.NextFloat(0.5f, 1f);
                    Vector2 sparkPos = Projectile.Center + swordDir * BladeLength * bladeT * Projectile.scale;
                    Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                    bool isBlack = Main.rand.NextBool();
                    Color sparkCol = isBlack ? new Color(30, 30, 45) : new Color(240, 240, 255);
                    Dust d = Dust.NewDustPerfect(sparkPos, DustID.RainbowTorch, sparkVel, 0, sparkCol, 0.8f);
                    d.noGravity = true;
                }
            }
        }

        private void GenerateTrailPoints()
        {
            float trailLength = Math.Min(Progression, 0.5f);
            float startProgress = Math.Max(0f, Progression - trailLength);

            for (int i = 0; i < TrailPointCount; i++)
            {
                float t = MathHelper.Lerp(startProgress, Progression, (float)i / (TrailPointCount - 1));
                float animValue = BlackSwanUtils.PiecewiseAnimation(t, CurrentAnimation);
                int flipSign = IsFlipped ? -1 : 1;
                float angle = BaseRotation + animValue * MaxAngle * Direction * flipSign;
                Vector2 dir = angle.ToRotationVector2();
                _trailPositions[i] = Projectile.Center + dir * BladeLength * Projectile.scale;
            }
        }

        #endregion

        #region Flare Spawning

        private void SpawnFlares(float rotation)
        {
            BlackSwanPlayer bsp = null;
            try { bsp = Owner.GetModPlayer<BlackSwanPlayer>(); } catch { }

            Vector2 swordDir = rotation.ToRotationVector2();
            Vector2 tipPos = Projectile.Center + swordDir * BladeLength * Projectile.scale;

            switch (ComboPhase)
            {
                case 0: // Entrechat — 3 feather projectiles in fan arc
                {
                    float spreadAngle = MathHelper.ToRadians(35f);
                    for (int i = 0; i < 3; i++)
                    {
                        float angleOffset = MathHelper.Lerp(-spreadAngle / 2f, spreadAngle / 2f,
                            3 > 1 ? (float)i / 2f : 0.5f);
                        Vector2 flareDir = (rotation + angleOffset).ToRotationVector2();
                        Vector2 flareVel = flareDir * 10f + Main.rand.NextVector2Circular(0.5f, 0.5f);

                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, flareVel,
                            ModContent.ProjectileType<BlackSwanFlareProj>(),
                            (int)(Projectile.damage * 0.4f), Projectile.knockBack * 0.3f,
                            Projectile.owner, ai0: 0f);

                        // Feather dust burst
                        bool isBlack = Main.rand.NextBool();
                        Dust d = Dust.NewDustPerfect(tipPos, isBlack ? DustID.Shadowflame : DustID.WhiteTorch,
                            flareVel * 0.2f, 0, isBlack ? new Color(30, 30, 40) : new Color(248, 245, 255), 1.0f);
                        d.noGravity = true;
                    }
                    break;
                }
                case 1: // Fouetté — radial flare AoE
                {
                    bool empowered = bsp?.IsEmpowered ?? false;
                    int count = empowered ? 8 : 5;
                    int flareDamage = empowered ? Projectile.damage * 2 : Projectile.damage;

                    if (empowered)
                        bsp?.ConsumeEmpowerment();

                    float spreadAngle = empowered ? MathHelper.ToRadians(60f) : MathHelper.ToRadians(50f);
                    for (int i = 0; i < count; i++)
                    {
                        float angleOffset = MathHelper.Lerp(-spreadAngle / 2f, spreadAngle / 2f,
                            count > 1 ? (float)i / (count - 1) : 0.5f);
                        Vector2 flareDir = (rotation + angleOffset).ToRotationVector2();
                        Vector2 flareVel = flareDir * 12f + Main.rand.NextVector2Circular(1f, 1f);

                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, flareVel,
                            ModContent.ProjectileType<BlackSwanFlareProj>(),
                            flareDamage, Projectile.knockBack * 0.5f,
                            Projectile.owner, ai0: empowered ? 1f : 0f);

                        // Spark dust
                        bool isBlack = i % 2 == 0;
                        Dust d = Dust.NewDustPerfect(tipPos, DustID.RainbowTorch, flareVel * 0.3f, 0,
                            isBlack ? new Color(30, 30, 45) : new Color(240, 240, 255), 0.6f);
                        d.noGravity = true;
                    }

                    if (empowered)
                    {
                        // Rainbow burst on empowered release
                        for (int i = 0; i < 12; i++)
                        {
                            float hue = (float)i / 12f;
                            Color rainbow = Main.hslToRgb(hue, 0.9f, 0.85f);
                            Vector2 burstVel = Main.rand.NextVector2CircularEdge(6f, 6f);
                            Dust d = Dust.NewDustPerfect(tipPos, DustID.RainbowTorch, burstVel, 0, rainbow, 0.9f);
                            d.noGravity = true;
                        }
                        SoundEngine.PlaySound(SoundID.Item119 with { Volume = 0.8f }, tipPos);
                    }
                    break;
                }
                case 2: // Grand Jeté — swan shockwave + feather rain
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, swordDir * 6f,
                        ModContent.ProjectileType<BlackSwanFlareProj>(),
                        Projectile.damage, Projectile.knockBack,
                        Projectile.owner, ai0: 2f);

                    for (int i = 0; i < 5; i++)
                    {
                        float xOffset = MathHelper.Lerp(-80f, 80f, (float)i / 4f);
                        Vector2 spawnPos = tipPos + new Vector2(xOffset, -200f);
                        Vector2 rainVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), 5f + Main.rand.NextFloat(2f));

                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, rainVel,
                            ModContent.ProjectileType<BlackSwanFlareProj>(),
                            (int)(Projectile.damage * 0.3f), Projectile.knockBack * 0.2f,
                            Projectile.owner, ai0: 0f);
                    }

                    // Shockwave dust burst — alternating black/white with rainbow accents
                    for (int i = 0; i < 16; i++)
                    {
                        Vector2 burstVel = Main.rand.NextVector2CircularEdge(8f, 8f);
                        bool isBlack = i % 2 == 0;
                        Color col = isBlack ? new Color(30, 30, 45) : new Color(240, 240, 255);
                        Dust d = Dust.NewDustPerfect(tipPos, DustID.RainbowTorch, burstVel, 0, col, 0.9f);
                        d.noGravity = true;
                    }
                    // Rainbow ring accent
                    for (int i = 0; i < 8; i++)
                    {
                        float hue = (float)i / 8f;
                        Color rainbow = Main.hslToRgb(hue, 0.85f, 0.8f);
                        Vector2 ringVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                        Dust d = Dust.NewDustPerfect(tipPos, DustID.RainbowTorch, ringVel, 0, rainbow, 0.7f);
                        d.noGravity = true;
                    }

                    // Smoke wisps
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 smokeVel = Main.rand.NextVector2Circular(4f, 4f);
                        bool isBlack = i % 2 == 0;
                        Dust d = Dust.NewDustPerfect(tipPos, DustID.Smoke, smokeVel, 150,
                            isBlack ? new Color(30, 30, 40) : new Color(200, 200, 210), 1.5f);
                        d.noGravity = true;
                    }

                    if (Owner.velocity.Y >= -2f)
                        Owner.velocity.Y -= 3f;

                    SoundEngine.PlaySound(SoundID.Item119 with { Pitch = 0.15f, Volume = 0.9f }, tipPos);
                    break;
                }
            }
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

        #region On Hit

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SwansMark>(), 300);

            try { Owner.GetModPlayer<BlackSwanPlayer>().RegisterHit(); } catch { }

            Vector2 hitPos = target.Center;

            // Base impact: dual-polarity spark burst with rainbow accents
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                bool isBlack = i % 2 == 0;
                Color col = isBlack ? new Color(30, 30, 45) : new Color(240, 240, 255);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.RainbowTorch, sparkVel, 0, col, 0.7f);
                d.noGravity = true;
            }

            // Feather dust burst
            for (int i = 0; i < 3; i++)
            {
                Vector2 featherVel = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0, -1.5f);
                bool isBlack = Main.rand.NextBool();
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(15f, 15f),
                    isBlack ? DustID.Shadowflame : DustID.WhiteTorch, featherVel, 0,
                    isBlack ? new Color(30, 30, 40) : new Color(248, 245, 255), 1.2f);
                d.noGravity = true;
            }

            // Rainbow accent sparkles
            for (int i = 0; i < 2 + ComboPhase; i++)
            {
                float hue = Main.rand.NextFloat();
                Color rainbow = Main.hslToRgb(hue, 0.85f, 0.8f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f));
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.RainbowTorch, noteVel, 0, rainbow, 0.8f);
                d.noGravity = true;
            }

            // Music notes from VFX library
            try { SwanLakeVFXLibrary.SpawnMusicNotes(hitPos, 2 + ComboPhase, 20f, 0.6f, 1.0f, 28); } catch { }

            // Phase-specific hit VFX
            switch (ComboPhase)
            {
                case 0: // Entrechat — feather fan from blade tip
                {
                    Vector2 swordDir = Projectile.rotation.ToRotationVector2();
                    Vector2 tipPos = Projectile.Center + swordDir * BladeLength * Projectile.scale;
                    for (int i = 0; i < 3; i++)
                    {
                        float fanAngle = Projectile.rotation + MathHelper.Lerp(-0.4f, 0.4f, i / 2f);
                        Vector2 featherDir = fanAngle.ToRotationVector2();
                        Vector2 vel = featherDir * 4f + new Vector2(0, -2f);
                        bool isBlack = Main.rand.NextBool();
                        Dust d = Dust.NewDustPerfect(tipPos + Main.rand.NextVector2Circular(5f, 5f),
                            isBlack ? DustID.Shadowflame : DustID.WhiteTorch, vel, 0,
                            isBlack ? new Color(30, 30, 40) : new Color(248, 245, 255), 1.1f);
                        d.noGravity = true;
                    }
                    break;
                }
                case 1: // Fouetté — radial flare
                {
                    try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(hitPos, 6, 25f); } catch { }
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f;
                        Vector2 radialVel = angle.ToRotationVector2() * 3.5f;
                        bool isBlack = i % 2 == 0;
                        Color col = isBlack ? new Color(30, 30, 45) : new Color(240, 240, 255);
                        Dust d = Dust.NewDustPerfect(hitPos, DustID.RainbowTorch, radialVel, 0, col, 0.8f);
                        d.noGravity = true;
                    }

                    // Prismatic Swan release?
                    BlackSwanPlayer bsp = null;
                    try { bsp = Owner.GetModPlayer<BlackSwanPlayer>(); } catch { }
                    if (bsp != null && bsp.IsMaxGrace)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            float hue = (float)i / 16f;
                            Color prismatic = Main.hslToRgb(hue, 0.9f, 0.85f);
                            Vector2 burstVel = Main.rand.NextVector2CircularEdge(8f, 8f);
                            Dust d = Dust.NewDustPerfect(hitPos, DustID.RainbowTorch, burstVel, 0, prismatic, 1.2f);
                            d.noGravity = true;
                        }
                        try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(hitPos, 10, 35f); } catch { }
                        SoundEngine.PlaySound(SoundID.Item119 with { Pitch = 0.3f, Volume = 0.9f }, hitPos);
                        bsp.ConsumePrismaticSwan();
                    }
                    break;
                }
                case 2: // Grand Jeté — shockwave
                {
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 12f;
                        Vector2 ringVel = angle.ToRotationVector2() * 6f;
                        bool isBlack = i % 2 == 0;
                        Color col = isBlack ? new Color(30, 30, 45) : new Color(240, 240, 255);
                        Dust d = Dust.NewDustPerfect(hitPos, DustID.RainbowTorch, ringVel, 0, col, 0.7f);
                        d.noGravity = true;
                    }

                    // Rainbow accent ring
                    for (int i = 0; i < 8; i++)
                    {
                        float hue = (float)i / 8f;
                        Color rainbow = Main.hslToRgb(hue, 0.85f, 0.8f);
                        Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f);
                        Dust d = Dust.NewDustPerfect(hitPos, DustID.RainbowTorch, vel, 0, rainbow, 0.9f);
                        d.noGravity = true;
                    }

                    // Feather rain from above
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 spawnPos = hitPos + new Vector2(Main.rand.NextFloat(-60f, 60f), -180f - Main.rand.NextFloat(40f));
                        Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), 3f + Main.rand.NextFloat(1.5f));
                        bool isBlack = Main.rand.NextBool();
                        Dust d = Dust.NewDustPerfect(spawnPos, isBlack ? DustID.Shadowflame : DustID.WhiteTorch,
                            vel, 0, isBlack ? new Color(30, 30, 40) : new Color(248, 245, 255), 1.3f);
                        d.noGravity = true;
                    }

                    // Smoke
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 smokeVel = Main.rand.NextVector2Circular(4f, 4f);
                        Dust d = Dust.NewDustPerfect(hitPos, DustID.Smoke, smokeVel, 150,
                            i % 2 == 0 ? new Color(30, 30, 40) : new Color(200, 200, 210), 1.5f);
                        d.noGravity = true;
                    }

                    if (hit.Crit)
                        try { SwanLakeVFXLibrary.MeleeImpact(hitPos, ComboPhase); } catch { }
                    break;
                }
            }
        }

        #endregion

        #region Rendering (Foundation Pattern — Safe SpriteBatch)

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            try
            {
                // Layer 1: Swing arc trail (additive bloom along trail positions)
                if (Progression > 0.20f && _trailPositions != null)
                    DrawSwingTrail(sb);

                // Layer 2: Bloom at blade tip (additive)
                DrawTipBloom(sb);
            }
            catch { }

            // Layer 3: Blade sprite (alpha blend — always executes)
            try
            {
                DrawBlade(sb, lightColor);
            }
            catch { }

            // Theme accents (additive)
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            BlackSwanUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private void DrawSwingTrail(SpriteBatch sb)
        {
            // Get bloom texture from registry
            Texture2D bloom = MagnumTextureRegistry.SoftGlow?.Value;
            if (bloom == null) return;

            float trailOpacity = MathHelper.Clamp((Progression - 0.20f) / 0.15f, 0f, 1f);
            if (Progression > 0.85f)
                trailOpacity *= 1f - (Progression - 0.85f) / 0.15f;

            if (trailOpacity <= 0f) return;

            // Get prismatic intensity from player state
            float prismatic = 0f;
            try { prismatic = Owner.GetModPlayer<BlackSwanPlayer>().PrismaticIntensity; } catch { }

            bool isBlackPhase = ComboPhase % 2 == 0;

            // Switch to additive
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 origin = bloom.Size() * 0.5f;

            // Draw bloom circles along the trail arc
            int step = Math.Max(1, TrailPointCount / 20);
            for (int i = 0; i < TrailPointCount; i += step)
            {
                float t = (float)i / (TrailPointCount - 1);
                Vector2 drawPos = _trailPositions[i] - Main.screenPosition;

                // Size tapers toward the start of the trail
                float size = MathHelper.Lerp(0.04f, 0.22f, t) * Projectile.scale * _squishFactor;

                // Dual-polarity: outer halo alternates black/white
                float localOpacity = trailOpacity * MathHelper.Lerp(0.2f, 0.8f, t);
                Color outerCol = isBlackPhase
                    ? new Color(50, 50, 65, 0)
                    : new Color(220, 225, 240, 0);

                // Outer halo
                sb.Draw(bloom, drawPos, null, outerCol * localOpacity * 0.4f, 0f, origin, size * 2.5f, SpriteEffects.None, 0f);

                // Mid white-silver glow
                Color midCol = new Color(200, 200, 215, 0);
                sb.Draw(bloom, drawPos, null, midCol * localOpacity * 0.5f, 0f, origin, size * 1.5f, SpriteEffects.None, 0f);

                // White core
                sb.Draw(bloom, drawPos, null, new Color(255, 255, 255, 0) * localOpacity * 0.6f, 0f, origin, size * 0.8f, SpriteEffects.None, 0f);

                // Rainbow accent edge (subtle, grows with Grace stacks)
                if (prismatic > 0f || i % 3 == 0)
                {
                    float hue = (t * 0.5f + Main.GameUpdateCount * 0.01f) % 1f;
                    Color rainbow = Main.hslToRgb(hue, 0.85f, 0.8f);
                    float rainbowOpacity = MathHelper.Lerp(0.08f, 0.35f, prismatic);
                    sb.Draw(bloom, drawPos, null, new Color(rainbow.R, rainbow.G, rainbow.B, 0) * localOpacity * rainbowOpacity,
                        0f, origin, size * 3.0f, SpriteEffects.None, 0f);
                }
            }

            // Restore SpriteBatch to alpha blend
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawTipBloom(SpriteBatch sb)
        {
            Texture2D bloom = MagnumTextureRegistry.SoftGlow?.Value;
            Texture2D pointBloom = MagnumTextureRegistry.PointBloom?.Value;
            if (bloom == null && pointBloom == null) return;

            float currentAngle = Projectile.rotation;
            Vector2 swordDir = currentAngle.ToRotationVector2();
            Vector2 tipPos = Projectile.Center + swordDir * BladeLength * Projectile.scale - Main.screenPosition;

            float bloomPulse = 0.8f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.15f);
            bool isBlackPhase = ComboPhase % 2 == 0;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Wide polarity halo
            if (bloom != null)
            {
                Vector2 origin = bloom.Size() * 0.5f;
                Color outerBloom = isBlackPhase ? new Color(30, 30, 50, 0) : new Color(200, 200, 220, 0);
                sb.Draw(bloom, tipPos, null, outerBloom * 0.25f * bloomPulse, 0f, origin, 0.6f * Projectile.scale, SpriteEffects.None, 0f);

                // Layer 2: Silver mid glow
                Color midBloom = new Color(180, 185, 210, 0);
                sb.Draw(bloom, tipPos, null, midBloom * 0.40f * bloomPulse, 0f, origin, 0.35f * Projectile.scale, SpriteEffects.None, 0f);
            }

            // Layer 3: White-hot core
            if (pointBloom != null)
            {
                Vector2 pbOrigin = pointBloom.Size() * 0.5f;
                sb.Draw(pointBloom, tipPos, null, new Color(255, 255, 255, 0) * 0.55f * bloomPulse, 0f, pbOrigin, 0.15f * Projectile.scale, SpriteEffects.None, 0f);
            }

            // Layer 4: Rainbow accent shimmer (rotating star)
            Texture2D star = MagnumTextureRegistry.GetStar4Soft();
            if (star != null)
            {
                Vector2 starOrigin = star.Size() * 0.5f;
                float hue = (Progression * 0.5f + Main.GameUpdateCount * 0.012f) % 1f;
                Color rainbowStar = Main.hslToRgb(hue, 0.85f, 0.8f);
                float starRot = Main.GameUpdateCount * 0.08f;
                sb.Draw(star, tipPos, null, new Color(rainbowStar.R, rainbowStar.G, rainbowStar.B, 0) * 0.3f * bloomPulse,
                    starRot, starOrigin, 0.12f * Projectile.scale, SpriteEffects.None, 0f);
            }

            // Empowered prismatic outer halo
            bool isEmpowered = false;
            try { isEmpowered = Owner.GetModPlayer<BlackSwanPlayer>().IsEmpowered; } catch { }
            if (isEmpowered && ComboPhase == 2 && bloom != null)
            {
                Vector2 origin = bloom.Size() * 0.5f;
                float hue = (Progression + Main.GameUpdateCount * 0.02f) % 1f;
                Color rainbow = Main.hslToRgb(hue, 0.9f, 0.85f);
                sb.Draw(bloom, tipPos, null, new Color(rainbow.R, rainbow.G, rainbow.B, 0) * 0.30f, 0f, origin, 0.8f * Projectile.scale, SpriteEffects.None, 0f);
            }

            // Restore
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawBlade(SpriteBatch sb, Color lightColor)
        {
            Texture2D bladeTex = TextureAssets.Projectile[Type].Value;
            if (bladeTex == null) return;

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

            // Polarity overlay glow
            bool isBlackPhase = ComboPhase % 2 == 0;
            Color glowColor = isBlackPhase ? new Color(30, 30, 45, 0) : new Color(240, 240, 255, 0);
            float glowIntensity = 0.15f + 0.1f * (float)Math.Sin(Main.GameUpdateCount * 0.1f);
            sb.Draw(bladeTex, drawPos, null, glowColor * glowIntensity, rot, origin, squishScale * 1.01f, effects, 0f);
        }

        #endregion

        public override void OnKill(int timeLeft)
        {
            try { Owner.GetModPlayer<BlackSwanPlayer>().IsSwinging = false; } catch { }
        }
    }
}
