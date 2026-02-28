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

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Projectiles
{
    /// <summary>
    /// InfernalGeyserProj  EAlt-fire charge-release projectile.
    /// Player channels in place, building charge visible as growing flame aura.
    /// On release (or at max charge), fires a massive concentrated column of bell fire forward.
    /// Deals up to 2.5x base damage at max charge. Pierces and applies heavy Resonant Toll.
    /// </summary>
    public class InfernalGeyserProj : ModProjectile
    {
        #region Properties

        private const int MaxChargeTicks = 60;
        private const int ReleaseBeamDuration = 30;
        private const float MinDamageMult = 0.8f;
        private const float MaxDamageMult = 2.5f;
        private const float BeamLength = 400f;
        private const float BeamWidth = 50f;

        private Player Owner => Main.player[Projectile.owner];

        private enum GeyserState { Charging, Released }
        private GeyserState _state;
        private float _chargeTime;
        private float _releaseTimer;
        private float _aimAngle;
        private bool _initialized;
        private IgnitionOfTheBellPrimitiveRenderer _trailRenderer;
        private int _baseDamage;

        #endregion

        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/IgnitionOfTheBell/IgnitionOfTheBell";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.timeLeft = MaxChargeTicks + ReleaseBeamDuration + 10;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                _trailRenderer = new IgnitionOfTheBellPrimitiveRenderer();
                _state = GeyserState.Charging;
                _chargeTime = 0;
                _releaseTimer = 0;
                _baseDamage = Projectile.damage;
                _aimAngle = (Main.MouseWorld - Owner.Center).ToRotation();
            }

            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            if (_state == GeyserState.Charging)
                RunCharging();
            else
                RunRelease();
        }

        private void RunCharging()
        {
            _chargeTime++;
            _aimAngle = (Main.MouseWorld - Owner.Center).ToRotation();
            Owner.direction = Math.Cos(_aimAngle) >= 0 ? 1 : -1;

            // Mark player as charging
            var tracker = Owner.IgnitionOfTheBell();
            tracker.IsCharging = true;
            tracker.ChargeLevel = Math.Min(_chargeTime, MaxChargeTicks);

            // Position near player
            Projectile.Center = Owner.Center + _aimAngle.ToRotationVector2() * 30f;
            Projectile.rotation = _aimAngle;

            // Growing flame aura
            float chargePercent = Math.Min(_chargeTime / MaxChargeTicks, 1f);
            SpawnChargeParticles(chargePercent);

            // Release conditions: channel released or max charge
            bool released = !Owner.channel || _chargeTime >= MaxChargeTicks;
            if (released && _chargeTime >= 10) // Min charge time
            {
                _state = GeyserState.Released;
                _releaseTimer = 0;

                float damageMult = MathHelper.Lerp(MinDamageMult, MaxDamageMult, chargePercent);
                Projectile.damage = (int)(_baseDamage * damageMult);

                // Release flash
                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new BellIgnitionFlashParticle(Projectile.Center, 12, 2f + chargePercent));
            }
            else if (released)
            {
                Projectile.Kill();
            }

            Lighting.AddLight(Projectile.Center, new Vector3(0.4f, 0.15f, 0.02f) * chargePercent);
        }

        private void RunRelease()
        {
            _releaseTimer++;
            if (_releaseTimer >= ReleaseBeamDuration)
            {
                Projectile.Kill();
                return;
            }

            float releaseProgress = _releaseTimer / ReleaseBeamDuration;

            // Lock aim
            Vector2 direction = _aimAngle.ToRotationVector2();
            Projectile.Center = Owner.Center + direction * (BeamLength * 0.5f);
            Projectile.rotation = _aimAngle;
            Owner.direction = Math.Cos(_aimAngle) >= 0 ? 1 : -1;

            // Scale hitbox to cover beam
            Projectile.width = (int)(BeamLength * (1f - releaseProgress * 0.3f));
            Projectile.height = (int)(BeamWidth * (1f - releaseProgress * 0.5f));

            // Geyser fire particles along beam
            SpawnGeyserParticles(releaseProgress, direction);

            float intensity = 0.8f * (1f - releaseProgress);
            Lighting.AddLight(Projectile.Center, new Vector3(0.7f, 0.3f, 0.05f) * intensity);
        }

        private void SpawnChargeParticles(float chargePercent)
        {
            int count = (int)(1 + chargePercent * 4);
            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat(20f, 50f + chargePercent * 40f);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;
                Vector2 vel = (Projectile.Center - pos).SafeDirectionTo(Projectile.Center) * Main.rand.NextFloat(1f, 3f);

                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new FlameJetParticle(pos, vel, Main.rand.NextFloat(0.3f + chargePercent * 0.5f, 0.8f),
                        Main.rand.Next(10, 20), 0.3f + chargePercent * 0.3f));
            }

            if (chargePercent > 0.5f && Main.rand.NextBool(3))
            {
                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new BellIgnitionFlashParticle(Projectile.Center, 6, 0.5f + chargePercent * 0.5f));
            }
        }

        private void SpawnGeyserParticles(float progress, Vector2 direction)
        {
            float fade = 1f - progress;
            int count = (int)(6 * fade);

            for (int i = 0; i < count; i++)
            {
                float along = Main.rand.NextFloat() * BeamLength;
                Vector2 beamPos = Owner.Center + direction * along;
                float perpAngle = _aimAngle + MathHelper.PiOver2;
                Vector2 offset = perpAngle.ToRotationVector2() * Main.rand.NextFloat(-BeamWidth * 0.4f, BeamWidth * 0.4f);

                Vector2 vel = direction * Main.rand.NextFloat(2f, 6f) + Main.rand.NextVector2Circular(1f, 1f);

                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new FlameJetParticle(beamPos + offset, vel, Main.rand.NextFloat(0.5f, 1f),
                        Main.rand.Next(10, 20), 0.6f * fade));
            }

            // Edge embers
            for (int i = 0; i < 2; i++)
            {
                float along = Main.rand.NextFloat() * BeamLength;
                Vector2 edgePos = Owner.Center + direction * along;
                float perpAngle = _aimAngle + MathHelper.PiOver2;
                float side = Main.rand.NextBool() ? 1f : -1f;
                edgePos += perpAngle.ToRotationVector2() * BeamWidth * 0.5f * side;

                Vector2 vel = perpAngle.ToRotationVector2() * side * Main.rand.NextFloat(1f, 3f);
                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new ThrustEmberParticle(edgePos, vel, Main.rand.NextFloat(0.4f, 0.8f), 15, 0.3f));
            }

            // Vanilla fire dust
            for (int i = 0; i < 3; i++)
            {
                float along = Main.rand.NextFloat() * BeamLength;
                Vector2 dustPos = Owner.Center + direction * along;
                Dust d = Dust.NewDustPerfect(dustPos + Main.rand.NextVector2Circular(15f, 15f),
                    DustID.Torch, direction * Main.rand.NextFloat(1f, 3f),
                    0, IgnitionOfTheBellUtils.GetMagmaFlicker(Main.rand.NextFloat()), 1.2f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 3);

            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new ThrustEmberParticle(target.Center, sparkVel, Main.rand.NextFloat(0.6f, 1f), 20, 0.4f));
            }

            IgnitionOfTheBellParticleHandler.SpawnParticle(
                new BellIgnitionFlashParticle(target.Center, 10, 1.5f));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            if (_state == GeyserState.Charging)
                DrawChargeAura(sb);
            else
                DrawGeyserBeam(sb);

            DrawGeyserParticles(sb);
            return false;
        }

        private void DrawChargeAura(SpriteBatch sb)
        {
            Texture2D bloomTex = null;
            try
            {
                bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }
            if (bloomTex == null) return;

            float chargePercent = Math.Min(_chargeTime / MaxChargeTicks, 1f);
            Vector2 screenPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(bloomTex.Width / 2f, bloomTex.Height / 2f);
            float pulse = 0.7f + 0.3f * (float)Math.Sin(_chargeTime * 0.15f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float scale = 0.5f + chargePercent * 1.5f;
            sb.Draw(bloomTex, screenPos, null,
                IgnitionOfTheBellUtils.Additive(new Color(200, 40, 0), 0.3f * chargePercent * pulse),
                0f, origin, scale * 1.5f, SpriteEffects.None, 0f);

            sb.Draw(bloomTex, screenPos, null,
                IgnitionOfTheBellUtils.Additive(new Color(255, 140, 30), 0.4f * chargePercent * pulse),
                0f, origin, scale, SpriteEffects.None, 0f);

            sb.Draw(bloomTex, screenPos, null,
                IgnitionOfTheBellUtils.Additive(new Color(255, 240, 210), 0.6f * chargePercent),
                0f, origin, scale * 0.3f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawGeyserBeam(SpriteBatch sb)
        {
            if (_trailRenderer == null) return;

            float releaseProgress = _releaseTimer / ReleaseBeamDuration;
            float fade = 1f - releaseProgress;
            Vector2 direction = _aimAngle.ToRotationVector2();

            int trailCount = 20;
            Vector2[] beamPositions = new Vector2[trailCount];
            for (int i = 0; i < trailCount; i++)
            {
                float t = i / (float)(trailCount - 1);
                beamPositions[i] = Owner.Center + direction * BeamLength * t;
            }

            MiscShaderData shader = IgnitionOfTheBellShaderLoader.GetGeyserShader();
            Color beamColor = IgnitionOfTheBellUtils.GetThrustGradient(0.65f);

            if (shader != null)
            {
                shader.UseColor(beamColor);
                shader.UseSecondaryColor(IgnitionOfTheBellUtils.GetThrustGradient(0.9f));
                try { shader.Shader.Parameters["uTime"]?.SetValue(Main.GameUpdateCount * 0.05f); } catch { }
            }

            var mainSettings = new IgnitionOfTheBellPrimitiveRenderer.IgnitionOfTheBellTrailSettings(
                width: (float t) =>
                {
                    float taper = (float)Math.Sin(t * Math.PI);
                    return BeamWidth * taper * fade;
                },
                trailColor: (float t) =>
                {
                    float alpha = (float)Math.Sin(t * Math.PI) * fade;
                    return Color.Lerp(beamColor, Color.Transparent, 1f - alpha);
                },
                shader: shader,
                smoothen: false
            );

            sb.End();
            _trailRenderer.RenderTrail(beamPositions, mainSettings, trailCount);

            // Glow pass
            var glowSettings = new IgnitionOfTheBellPrimitiveRenderer.IgnitionOfTheBellTrailSettings(
                width: (float t) => BeamWidth * 1.4f * (float)Math.Sin(t * Math.PI) * fade,
                trailColor: (float t) => IgnitionOfTheBellUtils.Additive(
                    IgnitionOfTheBellUtils.GetThrustGradient(0.85f),
                    0.2f * (float)Math.Sin(t * Math.PI) * fade),
                shader: shader,
                smoothen: false
            );

            _trailRenderer.RenderTrail(beamPositions, glowSettings, trailCount);

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Bloom at beam origin
            DrawBeamOriginBloom(sb, fade);
        }

        private void DrawBeamOriginBloom(SpriteBatch sb, float fade)
        {
            Texture2D bloomTex = null;
            try
            {
                bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }
            if (bloomTex == null) return;

            Vector2 screenPos = Owner.Center - Main.screenPosition;
            Vector2 origin = new Vector2(bloomTex.Width / 2f, bloomTex.Height / 2f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            sb.Draw(bloomTex, screenPos, null,
                IgnitionOfTheBellUtils.Additive(new Color(255, 100, 0), 0.4f * fade),
                0f, origin, 1.5f, SpriteEffects.None, 0f);

            sb.Draw(bloomTex, screenPos, null,
                IgnitionOfTheBellUtils.Additive(new Color(255, 240, 210), 0.6f * fade),
                0f, origin, 0.5f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawGeyserParticles(SpriteBatch sb)
        {
            IgnitionOfTheBellParticleHandler handler = ModContent.GetInstance<IgnitionOfTheBellParticleHandler>();
            handler?.DrawAllParticles(sb);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (_state == GeyserState.Charging) return false;

            Vector2 direction = _aimAngle.ToRotationVector2();
            Vector2 start = Owner.Center;
            Vector2 end = Owner.Center + direction * BeamLength;

            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                start, end, BeamWidth * 0.5f, ref collisionPoint);
        }

        public override void OnKill(int timeLeft)
        {
            _trailRenderer?.Dispose();

            // Farewell burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new ThrustEmberParticle(Owner.Center, burstVel, Main.rand.NextFloat(0.5f, 1f), 20, 0.35f));
            }
        }
    }
}
