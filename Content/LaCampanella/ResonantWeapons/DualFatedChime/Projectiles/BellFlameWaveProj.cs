using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Primitives;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Shaders;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using MagnumOpus.Content.FoundationWeapons.RibbonFoundation;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Projectiles
{
    /// <summary>
    /// Bell Flame Wave  EHoming fire projectile fired during Grand Toll (Phase 2).
    /// Tracks enemies with moderate homing. Applies Resonant Toll and triggers bell chime on impact.
    /// Leaves a blazing infernal trail with shader-driven rendering.
    /// </summary>
    public class BellFlameWaveProj : ModProjectile
    {
        #region Properties

        private const float HomingRange = 400f;
        private const float HomingStrength = 0.06f;
        private const float MaxSpeed = 14f;

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private DualFatedChimePrimitiveRenderer _trailRenderer;

        #endregion

        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/DualFatedChime/DualFatedChime";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 200;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                _trailRenderer = new DualFatedChimePrimitiveRenderer();
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            // Moderate homing
            NPC target = DualFatedChimeUtils.ClosestNPCAt(Projectile.Center, HomingRange);
            if (target != null)
            {
                Vector2 desiredDir = Projectile.Center.SafeDirectionTo(target.Center);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredDir * Projectile.velocity.Length(), HomingStrength);
            }

            if (Projectile.velocity.Length() > MaxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Fire dust trail
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.Torch, -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, DualFatedChimeUtils.GetFireFlicker(Main.rand.NextFloat()), 1f);
                d.noGravity = true;
                d.fadeIn = 0.7f;
            }

            // Black smoke wisps
            if (Main.rand.NextBool(4))
            {
                Dust s = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke,
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    60, new Color(20, 15, 20), 1.2f);
                s.noGravity = true;
            }

            // Pulsing fire light
            float intensity = 0.4f;
            float pulse = 1f + 0.2f * (float)Math.Sin(Projectile.timeLeft * 0.2f);
            Lighting.AddLight(Projectile.Center, new Vector3(0.7f, 0.3f, 0.05f) * intensity * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);

            // Impact VFX
            Vector2 hitPos = target.Center;
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                float heat = Main.rand.NextFloat(0.3f, 0.9f);
                DualFatedChimeParticleHandler.SpawnParticle(
                    new InfernalEmberParticle(hitPos, sparkVel, heat, 20, 0.4f));
            }

            // Bell chime flash
            DualFatedChimeParticleHandler.SpawnParticle(
                new BellChimeFlashParticle(hitPos, 12, 1.2f));

            // Smoke burst
            for (int i = 0; i < 2; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(2f, 2f);
                DualFatedChimeParticleHandler.SpawnParticle(
                    new BellSmokeParticle(hitPos, smokeVel, 30, 0.8f, 0.4f));
            }

            // === FOUNDATION: RippleEffectProjectile — Bell flame wave impact ring ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), hitPos, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner, ai0: 1f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                DrawFlameTrail(sb);
                DrawFlameCore(sb);
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false; // Don't draw default weapon sprite
        }

        private void DrawFlameTrail(SpriteBatch sb)
        {
            if (_trailRenderer == null) return;

            Vector2[] trailPositions = new Vector2[Projectile.oldPos.Length];
            for (int i = 0; i < trailPositions.Length; i++)
            {
                trailPositions[i] = Projectile.oldPos[i] == Vector2.Zero
                    ? Projectile.Center
                    : Projectile.oldPos[i] + Projectile.Size / 2f;
            }

            Color trailColor = DualFatedChimeUtils.GetInfernalGradient(0.5f);
            Color glowColor = DualFatedChimeUtils.GetInfernalGradient(0.8f);

            MiscShaderData shader = DualFatedChimeShaderLoader.GetFlameShader();
            if (shader != null)
            {
                shader.UseColor(trailColor);
                shader.UseSecondaryColor(glowColor);
                try { shader.Shader.Parameters["uTime"]?.SetValue(Main.GameUpdateCount * 0.03f); } catch { }
            }

            var mainSettings = new DualFatedChimeTrailSettings(
                width: (float t) => MathHelper.Lerp(14f, 2f, t),
                trailColor: (float t) => Color.Lerp(trailColor, Color.Transparent, t * t),
                shader: shader,
                smoothen: true
            );

            sb.End();
            _trailRenderer.RenderTrail(trailPositions, mainSettings, 30);

            var glowSettings = new DualFatedChimeTrailSettings(
                width: (float t) => MathHelper.Lerp(20f, 3f, t),
                trailColor: (float t) => DualFatedChimeUtils.Additive(glowColor, (1f - t) * 0.3f),
                shader: shader,
                smoothen: true
            );

            _trailRenderer.RenderTrail(trailPositions, glowSettings, 30);

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawFlameCore(SpriteBatch sb)
        {
            Texture2D bloomTex = null;
            try
            {
                bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }

            if (bloomTex == null) return;

            Vector2 screenPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(bloomTex.Width / 2f, bloomTex.Height / 2f);

            float pulse = 0.8f + 0.2f * (float)Math.Sin(Projectile.timeLeft * 0.15f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Orange fire glow
            sb.Draw(bloomTex, screenPos, null, DualFatedChimeUtils.Additive(new Color(255, 100, 0), 0.4f * pulse),
                0f, origin, 0.29f, SpriteEffects.None, 0f);

            // White-hot core
            sb.Draw(bloomTex, screenPos, null, DualFatedChimeUtils.Additive(new Color(255, 240, 200), 0.7f * pulse),
                0f, origin, 0.25f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                DualFatedChimeParticleHandler.SpawnParticle(
                    new InfernalEmberParticle(Projectile.Center, sparkVel, 0.6f, 15, 0.3f));
            }

            _trailRenderer?.Dispose();
        }
    }
}
