using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Primitives;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Shaders;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.LaCampanella;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using MagnumOpus.Content.FoundationWeapons.ExplosionParticlesFoundation;
using MagnumOpus.Content.FoundationWeapons.XSlashFoundation;
using static MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Utilities.IgnitionOfTheBellUtils;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Projectiles
{
    /// <summary>
    /// IgnitionThrustProj - 3-phase thrust combo projectile.
    /// Phase 0 = Ignition Strike (16 ticks, 120px, 1.0x dmg, spawns ground geyser on hit)
    /// Phase 1 = Tolling Frenzy (30 ticks, 100px, 0.85x dmg per sub-thrust, rapid triple thrust, smaller geysers)
    /// Phase 2 = Chime Cyclone (24 ticks spin, 140px reach, 1.3x dmg, spawns ChimeCycloneProj at end)
    /// Uses CurveSegment for thrust extension curves. Renders lance with flame trail.
    /// </summary>
    public class IgnitionThrustProj : ModProjectile
    {
        #region Properties

        private enum ThrustPhase { IgnitionStrike = 0, TollingFrenzy = 1, ChimeCyclone = 2 }

        private struct ThrustConfig
        {
            public int Duration;
            public float Reach;
            public float DamageMult;
            public CurveSegment[] ExtensionCurve;
        }

        // Phase 0: Ignition Strike - strong forward thrust
        // Phase 1: Tolling Frenzy - triple rapid thrust (3 sub-thrusts within duration)
        // Phase 2: Chime Cyclone - wide spin that spawns cyclone vortex
        private static readonly ThrustConfig[] Phases = new ThrustConfig[]
        {
            // Ignition Strike: powerful single thrust
            new ThrustConfig
            {
                Duration = 16,
                Reach = 120f,
                DamageMult = 1f,
                ExtensionCurve = new CurveSegment[]
                {
                    new CurveSegment(0f, 0.12f, 0f, 0.15f, SineInEasing),
                    new CurveSegment(0.12f, 0.4f, 0.15f, 1f, ExpOutEasing),
                    new CurveSegment(0.4f, 0.7f, 1f, 1f, LinearEasing),
                    new CurveSegment(0.7f, 1f, 1f, 0f, PolyInEasing),
                }
            },
            // Tolling Frenzy: triple-burst thrust (3 peaks in the curve)
            new ThrustConfig
            {
                Duration = 30,
                Reach = 100f,
                DamageMult = 0.85f,
                ExtensionCurve = new CurveSegment[]
                {
                    // Sub-thrust 1 (left)
                    new CurveSegment(0f, 0.08f, 0f, 0.9f, ExpOutEasing),
                    new CurveSegment(0.08f, 0.2f, 0.9f, 0.1f, PolyInEasing),
                    // Sub-thrust 2 (center)
                    new CurveSegment(0.2f, 0.35f, 0.1f, 1f, ExpOutEasing),
                    new CurveSegment(0.35f, 0.5f, 1f, 0.1f, PolyInEasing),
                    // Sub-thrust 3 (right)
                    new CurveSegment(0.5f, 0.65f, 0.1f, 1.05f, ExpOutEasing),
                    new CurveSegment(0.65f, 0.85f, 1.05f, 0.2f, SineInOutEasing),
                    new CurveSegment(0.85f, 1f, 0.2f, 0f, PolyInEasing),
                }
            },
            // Chime Cyclone: spin wind-up into cyclone spawn
            new ThrustConfig
            {
                Duration = 24,
                Reach = 140f,
                DamageMult = 1.3f,
                ExtensionCurve = new CurveSegment[]
                {
                    new CurveSegment(0f, 0.15f, 0.3f, 0.6f, SineInEasing),
                    new CurveSegment(0.15f, 0.5f, 0.6f, 1f, SineInOutEasing),
                    new CurveSegment(0.5f, 0.85f, 1f, 0.8f, SineInOutEasing),
                    new CurveSegment(0.85f, 1f, 0.8f, 0f, PolyInEasing),
                }
            }
        };

        private Player Owner => Main.player[Projectile.owner];
        private ThrustPhase Phase => (ThrustPhase)(int)Projectile.ai[0];
        private ThrustConfig Config => Phases[(int)Phase];
        private ref float Timer => ref Projectile.ai[1];

        private float _aimAngle;
        private bool _initialized;
        private IgnitionOfTheBellPrimitiveRenderer _trailRenderer;

        // Phase 1 (Tolling Frenzy): track sub-thrust angles (left/center/right)
        private float[] _frenzyAngles = new float[3];
        private int _lastSubThrust = -1;

        // Phase 2 (Chime Cyclone): spin angle
        private float _spinAngle;
        private bool _cycloneSpawned;

        #endregion

        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/IgnitionOfTheBell/IgnitionOfTheBell";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                _trailRenderer = new IgnitionOfTheBellPrimitiveRenderer();
                _aimAngle = (Main.MouseWorld - Owner.Center).ToRotation();
                Projectile.timeLeft = Config.Duration;
                Timer = 0;

                // Phase 1: pre-calculate 3 sub-thrust angles (left/center/right spread)
                if (Phase == ThrustPhase.TollingFrenzy)
                {
                    float spread = MathHelper.ToRadians(18f);
                    _frenzyAngles[0] = _aimAngle - spread;
                    _frenzyAngles[1] = _aimAngle;
                    _frenzyAngles[2] = _aimAngle + spread;
                }

                // Phase 2: initial spin angle
                if (Phase == ThrustPhase.ChimeCyclone)
                {
                    _spinAngle = _aimAngle;
                    _cycloneSpawned = false;
                }
            }

            Timer++;
            float progress = Timer / Config.Duration;

            if (progress >= 1f)
            {
                // Phase 2: Spawn Chime Cyclone at end
                if (Phase == ThrustPhase.ChimeCyclone && !_cycloneSpawned && Projectile.owner == Main.myPlayer)
                {
                    _cycloneSpawned = true;
                    Vector2 cyclonePos = Owner.Center + _aimAngle.ToRotationVector2() * Config.Reach * 0.7f;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        cyclonePos,
                        Vector2.Zero,
                        ModContent.ProjectileType<ChimeCycloneProj>(),
                        (int)(Projectile.damage * 0.8f),
                        0f,
                        Projectile.owner
                    );
                }

                Projectile.Kill();
                return;
            }

            // Calculate extension
            float extension = PiecewiseAnimation(progress, Config.ExtensionCurve);
            float reach = Config.Reach * extension;

            // Phase-specific behavior
            float currentAngle;
            if (Phase == ThrustPhase.TollingFrenzy)
            {
                // Determine which sub-thrust we're on based on progress
                int subThrust = progress < 0.2f ? 0 : progress < 0.5f ? 1 : 2;
                currentAngle = _frenzyAngles[subThrust];

                // Track sub-thrust transitions for geyser spawning
                if (subThrust != _lastSubThrust)
                {
                    _lastSubThrust = subThrust;
                    // Reset local NPC immunity for each sub-thrust
                    for (int i = 0; i < Main.maxNPCs; i++)
                        Projectile.localNPCImmunity[i] = 0;
                }
            }
            else if (Phase == ThrustPhase.ChimeCyclone)
            {
                // Spin: accelerating rotation
                float spinSpeed = 0.15f + progress * 0.4f;
                _spinAngle += spinSpeed;
                currentAngle = _spinAngle;
            }
            else
            {
                currentAngle = _aimAngle;
            }

            // Position at lance tip
            Vector2 direction = currentAngle.ToRotationVector2();
            Projectile.Center = Owner.Center + direction * Math.Max(reach, 0);
            Projectile.rotation = currentAngle;

            // Player facing
            Owner.direction = Math.Cos(currentAngle) >= 0 ? 1 : -1;
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            // Scale damage by phase
            Projectile.damage = (int)(Owner.GetWeaponDamage(Owner.HeldItem) * Config.DamageMult);

            // Flame particles
            SpawnThrustParticles(progress, direction, reach);

            // Lighting
            float intensity = Math.Max(extension, 0) * 0.6f;
            Lighting.AddLight(Projectile.Center, new Vector3(0.6f, 0.2f, 0.02f) * intensity);
        }

        private void SpawnThrustParticles(float progress, Vector2 direction, float reach)
        {
            // Flame jet stream behind the lance tip
            if (progress > 0.1f && progress < 0.85f)
            {
                int jetCount = Phase == ThrustPhase.ChimeCyclone ? 3 : 2;
                for (int i = 0; i < jetCount; i++)
                {
                    Vector2 jetPos = Owner.Center + direction * Math.Max(reach * Main.rand.NextFloat(0.3f, 1f), 0);
                    float perpAngle = Projectile.rotation + MathHelper.PiOver2;
                    Vector2 offset = perpAngle.ToRotationVector2() * Main.rand.NextFloat(-8f, 8f);
                    Vector2 jetVel = -direction * Main.rand.NextFloat(1f, 3f) + offset * 0.3f;

                    IgnitionOfTheBellParticleHandler.SpawnParticle(
                        new FlameJetParticle(jetPos + offset, jetVel, Main.rand.NextFloat(0.4f, 0.9f), 18, 0.5f));
                }
            }

            // Chime Cyclone: spinning fire ring
            if (Phase == ThrustPhase.ChimeCyclone && progress > 0.2f)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = _spinAngle + i * MathHelper.PiOver2 + Main.rand.NextFloat(-0.2f, 0.2f);
                    float radius = reach * Main.rand.NextFloat(0.5f, 1f);
                    Vector2 sparkPos = Owner.Center + angle.ToRotationVector2() * radius;
                    Vector2 sparkVel = (angle + MathHelper.PiOver2).ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                    IgnitionOfTheBellParticleHandler.SpawnParticle(
                        new ThrustEmberParticle(sparkPos, sparkVel, Main.rand.NextFloat(0.5f, 1f), 15, 0.35f));
                }
            }

            // Vanilla dust for density
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = Owner.Center + direction * Main.rand.NextFloat(0, Math.Max(reach, 0));
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch,
                    -direction * Main.rand.NextFloat(0.5f, 2f),
                    0, GetMagmaFlicker(Main.rand.NextFloat()), 0.9f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Resonant Toll
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);

            // === FOUNDATION: RippleEffectProjectile — Geyser eruption ring on every hit ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner);

            // Phase 0 (Ignition Strike): Spawn ground geyser at target + ExplosionParticles at geyser tip
            if (Phase == ThrustPhase.IgnitionStrike && Projectile.owner == Main.myPlayer)
            {
                Vector2 geyserPos = new Vector2(target.Center.X, target.Bottom.Y);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    geyserPos,
                    Vector2.Zero,
                    ModContent.ProjectileType<InfernalGeyserProj>(),
                    (int)(Projectile.damage * 0.6f),
                    2f,
                    Projectile.owner,
                    ai0: 0f // Full-size geyser
                );

                // === FOUNDATION: SparkExplosionProjectile — Geyser eruption sparks (upward bias) ===
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), geyserPos, Vector2.Zero,
                    ModContent.ProjectileType<SparkExplosionProjectile>(),
                    0, 0f, Projectile.owner,
                    ai0: (float)SparkMode.FountainCascade);
            }

            // Phase 1 (Tolling Frenzy): Spawn smaller geyser on each sub-thrust hit
            if (Phase == ThrustPhase.TollingFrenzy && Projectile.owner == Main.myPlayer)
            {
                Vector2 geyserPos = new Vector2(target.Center.X, target.Bottom.Y);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    geyserPos,
                    Vector2.Zero,
                    ModContent.ProjectileType<InfernalGeyserProj>(),
                    (int)(Projectile.damage * 0.35f),
                    1f,
                    Projectile.owner,
                    ai0: 1f // Small geyser variant
                );
            }

            // Impact VFX
            Vector2 hitPos = target.Center;
            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkDir = Projectile.rotation.ToRotationVector2() + Main.rand.NextVector2Circular(0.5f, 0.5f);
                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new ThrustEmberParticle(hitPos, sparkDir * Main.rand.NextFloat(3f, 6f),
                        Main.rand.NextFloat(0.5f, 1f), 20, 0.4f));
            }

            IgnitionOfTheBellParticleHandler.SpawnParticle(
                new BellIgnitionFlashParticle(hitPos, 10, 1f));
        }

        private static int _lastParticleDrawFrame;

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            try
            {
                DrawFlameTrail(sb);
                DrawBloomUnderlays(sb);
                DrawLanceSprite(sb, lightColor);

                // Particle dedup — only draw once per frame
                int currentFrame = (int)Main.GameUpdateCount;
                if (_lastParticleDrawFrame != currentFrame)
                {
                    _lastParticleDrawFrame = currentFrame;
                    DrawParticles(sb);
                }

                // Theme texture accents
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                IgnitionOfTheBellUtils.DrawThemeAccents(sb, Projectile.Center - Main.screenPosition, Projectile.scale);
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch
            {
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

        #region Rendering

        private void DrawFlameTrail(SpriteBatch sb)
        {
            if (_trailRenderer == null) return;

            float progress = Timer / Config.Duration;
            float extension = PiecewiseAnimation(progress, Config.ExtensionCurve);
            float reach = Config.Reach * extension;

            float currentAngle;
            if (Phase == ThrustPhase.ChimeCyclone)
                currentAngle = _spinAngle;
            else if (Phase == ThrustPhase.TollingFrenzy)
            {
                int subThrust = progress < 0.2f ? 0 : progress < 0.5f ? 1 : 2;
                currentAngle = _frenzyAngles[subThrust];
            }
            else
                currentAngle = _aimAngle;

            Vector2 direction = currentAngle.ToRotationVector2();

            int trailCount = 12;
            Vector2[] trailPositions = new Vector2[trailCount];
            for (int i = 0; i < trailCount; i++)
            {
                float t = i / (float)(trailCount - 1);
                trailPositions[i] = Owner.Center + direction * Math.Max(reach * t, 0);
            }

            MiscShaderData shader = IgnitionOfTheBellShaderLoader.GetThrustShader();
            Color trailColor = GetThrustGradient(0.6f);
            Color glowColor = GetThrustGradient(0.85f);

            if (shader != null)
            {
                shader.UseColor(trailColor);
                shader.UseSecondaryColor(glowColor);
                try { shader.Shader.Parameters["uTime"]?.SetValue(Main.GameUpdateCount * 0.04f); } catch { }
            }

            var mainSettings = new IgnitionOfTheBellPrimitiveRenderer.IgnitionOfTheBellTrailSettings(
                width: (float t) =>
                {
                    float tipTaper = 1f - (float)Math.Pow(t, 2);
                    float baseTaper = (float)Math.Pow(t, 0.3f);
                    float phaseWidth = Phase == ThrustPhase.ChimeCyclone ? 22f : 16f;
                    return phaseWidth * tipTaper * baseTaper * Math.Max(extension, 0);
                },
                trailColor: (float t) =>
                {
                    float alpha = (float)Math.Sin(t * Math.PI);
                    return Color.Lerp(trailColor, Color.Transparent, 1f - alpha);
                },
                shader: shader,
                smoothen: false
            );

            try { sb.End(); } catch { }

            try
            {
                _trailRenderer.RenderTrail(trailPositions, mainSettings, trailCount);

                var glowSettings = new IgnitionOfTheBellPrimitiveRenderer.IgnitionOfTheBellTrailSettings(
                    width: (float t) => 24f * (float)Math.Sin(t * Math.PI) * Math.Max(extension, 0),
                    trailColor: (float t) => Additive(glowColor, 0.2f * (float)Math.Sin(t * Math.PI)),
                    shader: shader,
                    smoothen: false
                );

                _trailRenderer.RenderTrail(trailPositions, glowSettings, trailCount);
            }
            finally
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        private void DrawBloomUnderlays(SpriteBatch sb)
        {
            Texture2D bloomTex = null;
            try
            {
                bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }
            if (bloomTex == null) return;

            float progress = Timer / Config.Duration;
            float extension = Math.Max(PiecewiseAnimation(progress, Config.ExtensionCurve), 0);
            Vector2 tipPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(bloomTex.Width / 2f, bloomTex.Height / 2f);

            try { sb.End(); } catch { }
            try
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                float pulse = 0.8f + 0.2f * (float)Math.Sin(Timer * 0.3f);

                // Crimson outer bloom
                sb.Draw(bloomTex, tipPos, null,
                    Additive(new Color(200, 40, 0), 0.25f * extension * pulse),
                    0f, origin, 1.0f, SpriteEffects.None, 0f);

                // Magma mid bloom
                sb.Draw(bloomTex, tipPos, null,
                    Additive(new Color(255, 120, 20), 0.35f * extension * pulse),
                    0f, origin, 0.55f, SpriteEffects.None, 0f);

                // White-hot core
                sb.Draw(bloomTex, tipPos, null,
                    Additive(new Color(255, 240, 210), 0.5f * extension),
                    0f, origin, 0.2f, SpriteEffects.None, 0f);

                // --- LC Power Effect Ring — infernal concentric ring around lance tip ---
                if (extension > 0.3f)
                {
                    float ringPulse = 0.6f + 0.4f * (float)Math.Sin(Timer * 0.25f);
                    LaCampanellaVFXLibrary.DrawPowerEffectRing(sb, tipPos,
                        0.35f * extension,
                        Timer * 0.03f,
                        0.25f * extension * ringPulse,
                        LaCampanellaPalette.InfernalOrange);
                }

                // --- LC Infernal Beam Ring — fiery halo during thrust peak ---
                if (extension > 0.6f)
                {
                    float beamPulse = 0.5f + 0.5f * (float)Math.Sin(Timer * 0.35f);
                    LaCampanellaVFXLibrary.DrawInfernalBeamRing(sb, tipPos,
                        0.28f * extension,
                        -Timer * 0.04f,
                        0.2f * beamPulse * extension,
                        LaCampanellaPalette.FlameYellow);
                }
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        private void DrawLanceSprite(SpriteBatch sb, Color lightColor)
        {
            Texture2D tex = null;
            try { tex = ModContent.Request<Texture2D>(Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value; }
            catch { }
            if (tex == null) return;

            float progress = Timer / Config.Duration;
            float extension = Math.Max(PiecewiseAnimation(progress, Config.ExtensionCurve), 0);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            float rot = Projectile.rotation + MathHelper.PiOver4;

            SpriteEffects fx = Owner.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float drawScale = 1f;

            // Shadow underneath
            Color shadow = new Color(0, 0, 0, 180) * (extension * 0.5f);
            sb.Draw(tex, drawPos + new Vector2(2, 2), null, shadow, rot, origin, drawScale, fx, 0f);

            // Main sprite
            sb.Draw(tex, drawPos, null, lightColor * extension, rot, origin, drawScale, fx, 0f);

            // Fire overlay on the blade
            if (extension > 0.3f)
            {
                try { sb.End(); } catch { }
                try
                {
                    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    Color fireOverlay = Additive(GetMagmaFlicker(), 0.3f * extension);
                    sb.Draw(tex, drawPos, null, fireOverlay, rot, origin, drawScale * 1.02f, fx, 0f);
                }
                catch { }
                finally
                {
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
        }

        private void DrawParticles(SpriteBatch sb)
        {
            IgnitionOfTheBellParticleHandler handler = ModContent.GetInstance<IgnitionOfTheBellParticleHandler>();
            handler?.DrawAllParticles(sb);
        }

        #endregion

        public override void OnKill(int timeLeft)
        {
            _trailRenderer?.Dispose();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float progress = Timer / Config.Duration;
            float extension = Math.Max(PiecewiseAnimation(progress, Config.ExtensionCurve), 0);
            float reach = Config.Reach * extension;

            if (Phase == ThrustPhase.ChimeCyclone)
            {
                // Circular collision for spin
                float dist = Vector2.Distance(Owner.Center, targetHitbox.Center.ToVector2());
                return dist < reach + 30f;
            }

            // Line collision from player to lance tip
            float currentAngle;
            if (Phase == ThrustPhase.TollingFrenzy)
            {
                int subThrust = progress < 0.2f ? 0 : progress < 0.5f ? 1 : 2;
                currentAngle = _frenzyAngles[subThrust];
            }
            else
                currentAngle = _aimAngle;

            Vector2 direction = currentAngle.ToRotationVector2();
            Vector2 start = Owner.Center;
            Vector2 end = Owner.Center + direction * reach;

            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 20f, ref collisionPoint);
        }
    }
}
