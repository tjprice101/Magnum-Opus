using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Primitives;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Shaders;
using MagnumOpus.Content.LaCampanella.Debuffs;
using static MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Utilities.IgnitionOfTheBellUtils;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Projectiles
{
    /// <summary>
    /// IgnitionThrustProj  E3-phase thrust combo projectile.
    /// Phase 0 = Quick Jab (14 ticks, 100px, 0.85x dmg)
    /// Phase 1 = Cross Thrust (16 ticks, 120px, 1.0x dmg, reversed grip)
    /// Phase 2 = Infernal Lunge (22 ticks, 160px, 1.3x dmg, with fire jet)
    /// Uses CurveSegment for thrust extension curves. Renders lance with flame trail.
    /// </summary>
    public class IgnitionThrustProj : ModProjectile
    {
        #region Properties

        private enum ThrustPhase { Jab = 0, Cross = 1, InfernalLunge = 2 }

        private struct ThrustConfig
        {
            public int Duration;
            public float Reach;
            public float DamageMult;
            public CurveSegment[] ExtensionCurve;
        }

        private static readonly ThrustConfig[] Phases = new ThrustConfig[]
        {
            // Jab: fast snap, short reach
            new ThrustConfig
            {
                Duration = 14,
                Reach = 100f,
                DamageMult = 0.85f,
                ExtensionCurve = new CurveSegment[]
                {
                    new CurveSegment(0f, 0.1f, 0f, 0.2f, SineInEasing),
                    new CurveSegment(0.1f, 0.4f, 0.2f, 1f, ExpOutEasing),
                    new CurveSegment(0.4f, 0.7f, 1f, 1f, LinearEasing),
                    new CurveSegment(0.7f, 1f, 1f, 0f, PolyInEasing),
                }
            },
            // Cross: medium lunge, slight delay
            new ThrustConfig
            {
                Duration = 16,
                Reach = 120f,
                DamageMult = 1f,
                ExtensionCurve = new CurveSegment[]
                {
                    new CurveSegment(0f, 0.15f, 0f, 0.1f, SineInEasing),
                    new CurveSegment(0.15f, 0.45f, 0.1f, 1f, PolyOutEasing),
                    new CurveSegment(0.45f, 0.75f, 1f, 0.95f, SineInOutEasing),
                    new CurveSegment(0.75f, 1f, 0.95f, 0f, PolyInEasing),
                }
            },
            // Infernal Lunge: long windup, massive extension, fire jet
            new ThrustConfig
            {
                Duration = 22,
                Reach = 160f,
                DamageMult = 1.3f,
                ExtensionCurve = new CurveSegment[]
                {
                    new CurveSegment(0f, 0.2f, -0.15f, 0f, SineInOutEasing),
                    new CurveSegment(0.2f, 0.5f, 0f, 1.1f, ExpOutEasing),
                    new CurveSegment(0.5f, 0.8f, 1.1f, 1f, SineBumpEasing),
                    new CurveSegment(0.8f, 1f, 1f, 0f, PolyInEasing),
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
            }

            Timer++;
            float progress = Timer / Config.Duration;

            if (progress >= 1f)
            {
                Projectile.Kill();
                return;
            }

            // Calculate extension
            float extension = PiecewiseAnimation(progress, Config.ExtensionCurve);
            float reach = Config.Reach * extension;

            // Position at lance tip
            Vector2 direction = _aimAngle.ToRotationVector2();
            Projectile.Center = Owner.Center + direction * Math.Max(reach, 0);
            Projectile.rotation = _aimAngle;

            // Player facing
            Owner.direction = Math.Cos(_aimAngle) >= 0 ? 1 : -1;
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            // Scale damage by phase
            Projectile.damage = (int)(Owner.GetWeaponDamage(Owner.HeldItem) * Config.DamageMult);

            // Flame jet particles (concentrated along thrust direction)
            SpawnThrustParticles(progress, direction, reach);

            // Lighting
            float intensity = Math.Max(extension, 0) * 0.6f;
            Lighting.AddLight(Projectile.Center, new Vector3(0.6f, 0.2f, 0.02f) * intensity);
        }

        private void SpawnThrustParticles(float progress, Vector2 direction, float reach)
        {
            // Flame jet stream behind the lance tip
            if (progress > 0.15f && progress < 0.8f)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 jetPos = Owner.Center + direction * Math.Max(reach * Main.rand.NextFloat(0.3f, 1f), 0);
                    float perpAngle = _aimAngle + MathHelper.PiOver2;
                    Vector2 offset = perpAngle.ToRotationVector2() * Main.rand.NextFloat(-8f, 8f);
                    Vector2 jetVel = -direction * Main.rand.NextFloat(1f, 3f) + offset * 0.3f;

                    IgnitionOfTheBellParticleHandler.SpawnParticle(
                        new FlameJetParticle(jetPos + offset, jetVel, Main.rand.NextFloat(0.4f, 0.9f), 18, 0.5f));
                }
            }

            // Infernal Lunge: extra fire ring at peak extension
            if (Phase == ThrustPhase.InfernalLunge && progress > 0.35f && progress < 0.55f)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                    IgnitionOfTheBellParticleHandler.SpawnParticle(
                        new ThrustEmberParticle(Projectile.Center, sparkVel, Main.rand.NextFloat(0.5f, 1f), 20, 0.35f));
                }

                if (Main.rand.NextBool(3))
                {
                    IgnitionOfTheBellParticleHandler.SpawnParticle(
                        new BellIgnitionFlashParticle(Projectile.Center, 8, 0.8f));
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

            // Register cyclone hit
            var tracker = Owner.IgnitionOfTheBell();
            bool cycloneTriggered = tracker.RegisterHit(target.whoAmI);

            if (cycloneTriggered && Projectile.owner == Main.myPlayer)
            {
                // Spawn Chime Cyclone on target
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<ChimeCycloneProj>(),
                    (int)(Projectile.damage * 0.7f),
                    0f,
                    Projectile.owner
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

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            DrawFlameTrail(sb);
            DrawBloomUnderlays(sb);
            DrawLanceSprite(sb, lightColor);
            DrawParticles(sb);

            return false;
        }

        #region Rendering

        private void DrawFlameTrail(SpriteBatch sb)
        {
            if (_trailRenderer == null) return;

            // Build trail from thrust line (player center to tip)
            float progress = Timer / Config.Duration;
            float extension = PiecewiseAnimation(progress, Config.ExtensionCurve);
            float reach = Config.Reach * extension;
            Vector2 direction = _aimAngle.ToRotationVector2();

            // Create trail positions along the thrust line
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

            // Main flame trail
            var mainSettings = new IgnitionOfTheBellPrimitiveRenderer.IgnitionOfTheBellTrailSettings(
                width: (float t) =>
                {
                    float tipTaper = 1f - (float)Math.Pow(t, 2);
                    float baseTaper = (float)Math.Pow(t, 0.3f);
                    return 16f * tipTaper * baseTaper * Math.Max(extension, 0);
                },
                trailColor: (float t) =>
                {
                    float alpha = (float)Math.Sin(t * Math.PI);
                    return Color.Lerp(trailColor, Color.Transparent, 1f - alpha);
                },
                shader: shader,
                smoothen: false
            );

            sb.End();
            _trailRenderer.RenderTrail(trailPositions, mainSettings, trailCount);

            // Glow trail
            var glowSettings = new IgnitionOfTheBellPrimitiveRenderer.IgnitionOfTheBellTrailSettings(
                width: (float t) => 24f * (float)Math.Sin(t * Math.PI) * Math.Max(extension, 0),
                trailColor: (float t) => Additive(glowColor, 0.2f * (float)Math.Sin(t * Math.PI)),
                shader: shader,
                smoothen: false
            );

            _trailRenderer.RenderTrail(trailPositions, glowSettings, trailCount);

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawBloomUnderlays(SpriteBatch sb)
        {
            Texture2D bloomTex = null;
            try
            {
                bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }
            if (bloomTex == null) return;

            float progress = Timer / Config.Duration;
            float extension = Math.Max(PiecewiseAnimation(progress, Config.ExtensionCurve), 0);
            Vector2 tipPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(bloomTex.Width / 2f, bloomTex.Height / 2f);

            sb.End();
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

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
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
            float rot = _aimAngle + MathHelper.PiOver4;

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
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Color fireOverlay = Additive(GetMagmaFlicker(), 0.3f * extension);
                sb.Draw(tex, drawPos, null, fireOverlay, rot, origin, drawScale * 1.02f, fx, 0f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
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
            // Line collision from player to lance tip
            float progress = Timer / Config.Duration;
            float extension = Math.Max(PiecewiseAnimation(progress, Config.ExtensionCurve), 0);
            float reach = Config.Reach * extension;
            Vector2 direction = _aimAngle.ToRotationVector2();

            Vector2 start = Owner.Center;
            Vector2 end = Owner.Center + direction * reach;

            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 20f, ref collisionPoint);
        }
    }
}
