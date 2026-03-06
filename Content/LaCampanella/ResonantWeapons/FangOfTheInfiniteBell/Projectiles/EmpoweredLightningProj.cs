using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Primitives;
using MagnumOpus.Content.LaCampanella;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Projectiles
{
    /// <summary>
    /// EmpoweredLightningProj  ELightning bolt spawned on hit during empowered state.
    /// Strikes from above with jagged electric gold visuals. Applies Resonant Toll.
    /// ai[0] = target X, ai[1] = target Y.
    /// </summary>
    public class EmpoweredLightningProj : ModProjectile
    {
        private const int Duration = 20;
        private bool _initialized;
        private Vector2 _startPos;
        private Vector2 _targetPos;
        private Vector2[] _lightningPath;
        private FangOfTheInfiniteBellPrimitiveRenderer _trailRenderer;

        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/FangOfTheInfiniteBell/FangOfTheInfiniteBell";

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                _startPos = Projectile.Center;
                _targetPos = new Vector2(Projectile.ai[0], Projectile.ai[1]);
                _trailRenderer = new FangOfTheInfiniteBellPrimitiveRenderer();

                // Generate jagged lightning path
                GenerateLightningPath();

                // Initial impact flash
                FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                    new ArcaneFlashParticle(_targetPos, 12, 1.8f, true));

                // Scatter sparks at impact
                for (int i = 0; i < 8; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(5f, 5f);
                    FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                        new EmpoweredSparkParticle(_targetPos, vel, 18, 0.35f));
                }

                // === FOUNDATION: RippleEffectProjectile — Lightning strike ring ===
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), _targetPos, Vector2.Zero,
                    ModContent.ProjectileType<RippleEffectProjectile>(),
                    0, 0f, Projectile.owner, ai0: 1f);
            }

            // Position at target for collision
            Projectile.Center = _targetPos;

            // Sparks along bolt
            if (Main.rand.NextBool(2) && _lightningPath != null)
            {
                int idx = Main.rand.Next(_lightningPath.Length);
                Vector2 sparkPos = _lightningPath[idx];
                Vector2 sparkVel = Main.rand.NextVector2Circular(2f, 2f);
                FangOfTheInfiniteBellParticleHandler.SpawnParticle(
                    new EmpoweredSparkParticle(sparkPos, sparkVel, 12, 0.2f));
            }

            // Intense light at target
            float fade = (float)Projectile.timeLeft / Duration;
            Lighting.AddLight(_targetPos, new Vector3(0.7f, 0.6f, 0.2f) * fade);
        }

        private void GenerateLightningPath()
        {
            int segments = 12;
            _lightningPath = new Vector2[segments];
            Vector2 dir = _targetPos - _startPos;
            float totalLen = dir.Length();
            if (totalLen < 1f) totalLen = 1f;
            Vector2 normalizedDir = dir / totalLen;
            Vector2 perp = new(-normalizedDir.Y, normalizedDir.X);

            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)(segments - 1);
                Vector2 basePos = Vector2.Lerp(_startPos, _targetPos, t);

                // Jagged displacement (stronger in middle, none at endpoints)
                float jitterStrength = 30f * (float)Math.Sin(t * Math.PI);
                float jitter = Main.rand.NextFloat(-1f, 1f) * jitterStrength;
                _lightningPath[i] = basePos + perp * jitter;
            }

            // Ensure endpoints are exact
            _lightningPath[0] = _startPos;
            _lightningPath[segments - 1] = _targetPos;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (_lightningPath == null) return false;

            // Check collision along lightning bolt segments
            for (int i = 0; i < _lightningPath.Length - 1; i++)
            {
                float point = 0f;
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                    _lightningPath[i], _lightningPath[i + 1], 20f, ref point))
                    return true;
            }
            return false;
        }

        private static int _lastParticleDrawFrame;

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                DrawLightningBolt(sb);
                DrawImpactBloom(sb);

                int currentFrame = (int)Main.GameUpdateCount;
                if (_lastParticleDrawFrame != currentFrame)
                {
                    _lastParticleDrawFrame = currentFrame;
                    FangOfTheInfiniteBellParticleHandler handler = ModContent.GetInstance<FangOfTheInfiniteBellParticleHandler>();
                    handler?.DrawAllParticles(sb);
                }
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

        private void DrawLightningBolt(SpriteBatch sb)
        {
            if (_trailRenderer == null || _lightningPath == null) return;

            float fade = (float)Projectile.timeLeft / Duration;
            Color boltColor = FangOfTheInfiniteBellUtils.GetEmpoweredGradient(0.55f);

            var shader = Shaders.FangOfTheInfiniteBellShaderLoader.GetLightningShader();
            if (shader != null)
            {
                shader.UseColor(boltColor);
                shader.UseSecondaryColor(Color.White);
                try { shader.Shader.Parameters["uTime"]?.SetValue(Main.GameUpdateCount * 0.06f); } catch { }
            }

            // Main bolt
            var mainSettings = new FangOfTheInfiniteBellPrimitiveRenderer.FangTrailSettings(
                width: t => MathHelper.Lerp(8f, 3f, t) * fade,
                color: t =>
                {
                    float alpha = (float)Math.Sin(t * Math.PI) * fade;
                    return Color.Lerp(boltColor, Color.White, 0.3f) * alpha;
                },
                shader: shader,
                smoothen: false);

            try { sb.End(); } catch { }
            _trailRenderer.RenderTrail(_lightningPath, mainSettings, _lightningPath.Length);

            // Glow pass
            var glowSettings = new FangOfTheInfiniteBellPrimitiveRenderer.FangTrailSettings(
                width: t => MathHelper.Lerp(16f, 6f, t) * fade,
                color: t => FangOfTheInfiniteBellUtils.Additive(boltColor, 0.2f * (float)Math.Sin(t * Math.PI) * fade),
                shader: shader,
                smoothen: false);

            _trailRenderer.RenderTrail(_lightningPath, glowSettings, _lightningPath.Length);

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawImpactBloom(SpriteBatch sb)
        {
            Texture2D bloomTex = null;
            try { bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value; } catch { }
            if (bloomTex == null) return;

            float fade = (float)Projectile.timeLeft / Duration;
            Vector2 screenPos = _targetPos - Main.screenPosition;
            Vector2 origin = new(bloomTex.Width / 2f, bloomTex.Height / 2f);

            try { sb.End(); } catch { }
            try
            {
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            sb.Draw(bloomTex, screenPos, null,
                FangOfTheInfiniteBellUtils.Additive(new Color(255, 220, 50), 0.5f * fade),
                0f, origin, 1f * fade, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, screenPos, null,
                FangOfTheInfiniteBellUtils.Additive(new Color(255, 255, 200), 0.7f * fade),
                0f, origin, 0.35f * fade, SpriteEffects.None, 0f);

            // --- LC Radial Slash Star — sharp impact star at lightning strike point ---
            float starRot = (float)Main.GameUpdateCount * 0.06f;
            LaCampanellaVFXLibrary.DrawRadialSlashStar(sb, screenPos,
                0.3f * fade, starRot, 0.4f * fade,
                LaCampanellaPalette.FlameYellow);

            // --- LC Beam Lens Flare — bright explosion flare at impact ---
            if (fade > 0.5f)
            {
                LaCampanellaVFXLibrary.DrawBeamLensFlare(sb, screenPos,
                    0.2f * fade, 0f, 0.25f * (fade - 0.5f) * 2f,
                    LaCampanellaPalette.WhiteHot);
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

        public override void OnKill(int timeLeft)
        {
            _trailRenderer?.Dispose();
        }
    }
}
