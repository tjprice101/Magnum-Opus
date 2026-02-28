using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Primitives;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Shaders;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Projectiles
{
    /// <summary>
    /// InfiniteBellOrbProj  EHoming arcane bell-fire orb.
    /// Tracks enemies with moderate homing. On hit triggers empowerment cycle.
    /// When empowered (ai[0]=1), spawns EmpoweredLightningProj on hit.
    /// Arcane violet-fire trail with golden empowered variant.
    /// </summary>
    public class InfiniteBellOrbProj : ModProjectile
    {
        private const float HomingRange = 500f;
        private const float HomingStrength = 0.05f;
        private const float MaxSpeed = 16f;

        private Player Owner => Main.player[Projectile.owner];
        private bool IsEmpowered => Projectile.ai[0] > 0f;
        private bool _initialized;
        private FangOfTheInfiniteBellPrimitiveRenderer _trailRenderer;

        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/FangOfTheInfiniteBell/FangOfTheInfiniteBell";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 18;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                _trailRenderer = new FangOfTheInfiniteBellPrimitiveRenderer();
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            // Homing
            NPC target = FangOfTheInfiniteBellUtils.ClosestNPCAt(Projectile.Center, HomingRange);
            if (target != null)
            {
                Vector2 desired = Projectile.Center.SafeDirectionTo(target.Center);
                float strength = IsEmpowered ? HomingStrength * 1.5f : HomingStrength;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired * Projectile.velocity.Length(), strength);
            }

            if (Projectile.velocity.Length() > MaxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail particles
            if (Main.rand.NextBool(2))
            {
                Color dustColor = IsEmpowered
                    ? FangOfTheInfiniteBellUtils.GetEmpoweredFlicker(Main.rand.NextFloat())
                    : FangOfTheInfiniteBellUtils.GetArcaneFlicker(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.Torch, -Projectile.velocity * 0.15f, 0, dustColor, 0.9f);
                d.noGravity = true;
            }

            // Empowered: extra electric sparks
            if (IsEmpowered && Main.rand.NextBool(3))
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(1f, 1f);
                FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                    new EmpoweredSparkParticle(Projectile.Center, sparkVel, 12, 0.25f));
            }

            float intensity = IsEmpowered ? 0.6f : 0.35f;
            float pulse = 1f + 0.2f * (float)Math.Sin(Projectile.timeLeft * 0.2f);
            Vector3 lightColor = IsEmpowered
                ? new Vector3(0.6f, 0.5f, 0.1f)
                : new Vector3(0.5f, 0.2f, 0.1f);
            Lighting.AddLight(Projectile.Center, lightColor * intensity * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);

            // Register empowerment hit
            var tracker = Owner.FangOfTheInfiniteBell();
            tracker.ResetHitDecay();
            bool empowered = tracker.RegisterHit();

            // Impact VFX
            Vector2 hitPos = target.Center;
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f);
                FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                    new ArcaneOrbParticle(hitPos, vel, Main.rand.NextFloat(0.4f, 0.9f), 18, 0.35f));
            }

            FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                new ArcaneFlashParticle(hitPos, 10, 1f, IsEmpowered));

            // Music note on hit
            if (Main.rand.NextBool(2))
            {
                Vector2 noteVel = Vector2.UnitY * -Main.rand.NextFloat(0.5f, 1.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                    new MusicalBellNoteParticle(hitPos, noteVel, 30, 0.4f, IsEmpowered));
            }

            // When empowered, spawn lightning
            if (IsEmpowered && Projectile.owner == Main.myPlayer)
            {
                Vector2 lightningStart = target.Center + new Vector2(Main.rand.NextFloat(-80f, 80f), -300f);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), lightningStart, Vector2.Zero,
                    ModContent.ProjectileType<EmpoweredLightningProj>(),
                    Projectile.damage / 2, 0f, Projectile.owner,
                    target.Center.X, target.Center.Y);
            }

            // Empowerment trigger flash
            if (empowered)
            {
                FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                    new ArcaneFlashParticle(Owner.Center, 15, 2.5f, true));
                for (int i = 0; i < 8; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                    FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                        new EmpoweredSparkParticle(Owner.Center, sparkVel, 25, 0.4f));
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            DrawOrbTrail(sb);
            DrawOrbCore(sb);
            return false;
        }

        private void DrawOrbTrail(SpriteBatch sb)
        {
            if (_trailRenderer == null) return;

            Vector2[] trailPos = new Vector2[Projectile.oldPos.Length];
            for (int i = 0; i < trailPos.Length; i++)
                trailPos[i] = Projectile.oldPos[i] == Vector2.Zero ? Projectile.Center : Projectile.oldPos[i] + Projectile.Size / 2f;

            Color trailColor = IsEmpowered
                ? FangOfTheInfiniteBellUtils.GetEmpoweredGradient(0.5f)
                : FangOfTheInfiniteBellUtils.GetArcaneGradient(0.5f);

            MiscShaderData shader = FangOfTheInfiniteBellShaderLoader.GetOrbShader();
            if (shader != null)
            {
                shader.UseColor(trailColor);
                shader.UseSecondaryColor(Color.Lerp(trailColor, Color.White, 0.3f));
                try { shader.Shader.Parameters["uTime"]?.SetValue(Main.GameUpdateCount * 0.03f); } catch { }
            }

            var settings = new FangOfTheInfiniteBellPrimitiveRenderer.FangTrailSettings(
                width: t => MathHelper.Lerp(IsEmpowered ? 16f : 12f, 2f, t),
                color: t => Color.Lerp(trailColor, Color.Transparent, t * t),
                shader: shader);

            sb.End();
            _trailRenderer.RenderTrail(trailPos, settings, 30);

            var glowSettings = new FangOfTheInfiniteBellPrimitiveRenderer.FangTrailSettings(
                width: t => MathHelper.Lerp(IsEmpowered ? 22f : 18f, 3f, t),
                color: t => FangOfTheInfiniteBellUtils.Additive(trailColor, (1f - t) * 0.25f),
                shader: shader);

            _trailRenderer.RenderTrail(trailPos, glowSettings, 30);

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawOrbCore(SpriteBatch sb)
        {
            Texture2D bloomTex = null;
            try { bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow", ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value; } catch { }
            if (bloomTex == null) return;

            Vector2 screenPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new(bloomTex.Width / 2f, bloomTex.Height / 2f);
            float pulse = 0.8f + 0.2f * (float)Math.Sin(Projectile.timeLeft * 0.15f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (IsEmpowered)
            {
                sb.Draw(bloomTex, screenPos, null, FangOfTheInfiniteBellUtils.Additive(new Color(200, 150, 0), 0.4f * pulse), 0f, origin, 0.7f, SpriteEffects.None, 0f);
                sb.Draw(bloomTex, screenPos, null, FangOfTheInfiniteBellUtils.Additive(new Color(255, 255, 150), 0.7f * pulse), 0f, origin, 0.3f, SpriteEffects.None, 0f);
            }
            else
            {
                sb.Draw(bloomTex, screenPos, null, FangOfTheInfiniteBellUtils.Additive(new Color(180, 50, 20), 0.35f * pulse), 0f, origin, 0.55f, SpriteEffects.None, 0f);
                sb.Draw(bloomTex, screenPos, null, FangOfTheInfiniteBellUtils.Additive(new Color(255, 210, 100), 0.6f * pulse), 0f, origin, 0.22f, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(2f, 2f);
                FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                    new ArcaneOrbParticle(Projectile.Center, vel, 0.5f, 15, 0.25f));
            }
            _trailRenderer?.Dispose();
        }
    }
}
