using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Utilities;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Particles;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Primitives;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Shaders;
using MagnumOpus.Content.SwanLake.Debuffs;
using ReLogic.Content;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Projectiles
{
    /// <summary>
    /// Black Swan Flare — Homing sub-projectile fired during Phase 2 (Grand Jeté).
    /// Dual-polarity: randomly black or white on spawn. Tracks enemies.
    /// On hit: registers flare hit for empowerment system + visual impact.
    /// 
    /// ai[0] = 1 means empowered version (2× damage, rainbow aura).
    /// ai[1] = polarity (0 = white, 1 = black).
    /// </summary>
    public class BlackSwanFlareProj : ModProjectile
    {
        #region Properties

        public bool IsEmpowered => Projectile.ai[0] >= 1f;
        public bool IsBlack => Projectile.ai[1] >= 1f;

        private const float HomingRange = 350f;
        private const float HomingStrength = 0.08f;
        private const float MaxSpeed = 16f;

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private BlackSwanPrimitiveRenderer _trailRenderer;

        #endregion

        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/CalloftheBlackSwan/CalloftheBlackSwan";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                // Random polarity
                Projectile.ai[1] = Main.rand.NextBool() ? 1f : 0f;
                _trailRenderer = new BlackSwanPrimitiveRenderer();
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            // Homing AI
            NPC target = BlackSwanUtils.ClosestNPCAt(Projectile.Center, HomingRange);
            if (target != null)
            {
                Vector2 desiredDir = Projectile.Center.SafeDirectionTo(target.Center);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredDir * Projectile.velocity.Length(), HomingStrength);
            }

            // Cap speed
            if (Projectile.velocity.Length() > MaxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail dust
            if (Main.rand.NextBool(3))
            {
                int dustType = IsBlack ? DustID.Shadowflame : DustID.WhiteTorch;
                Color dustColor = IsBlack ? new Color(30, 30, 45) : new Color(240, 240, 255);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Empowered rainbow sparkle
            if (IsEmpowered && Main.rand.NextBool(4))
            {
                Color rainbow = BlackSwanUtils.GetRainbow(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2,
                    Main.rand.NextVector2Circular(1f, 1f), 0, rainbow, 0.5f);
                d.noGravity = true;
            }

            // Pulsing light
            float intensity = IsEmpowered ? 0.6f : 0.35f;
            float pulse = 1f + 0.15f * (float)Math.Sin(Projectile.timeLeft * 0.2f);
            Vector3 lightColor = IsBlack
                ? new Vector3(0.15f, 0.15f, 0.25f)
                : new Vector3(0.5f, 0.5f, 0.6f);
            Lighting.AddLight(Projectile.Center, lightColor * intensity * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Register flare hit for empowerment
            Owner.BlackSwan().RegisterFlareHit();

            // Apply debuff
            target.AddBuff(ModContent.BuffType<FlameOfTheSwan>(), 240); // 4 seconds

            // Impact VFX
            Vector2 hitPos = target.Center;
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                BlackSwanParticleHandler.SpawnParticle(
                    new DualitySparkParticle(hitPos, sparkVel, i % 2 == 0, 18, 0.5f));
            }

            // Feather burst on impact
            for (int i = 0; i < 2; i++)
            {
                Vector2 featherVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                BlackSwanParticleHandler.SpawnParticle(
                    new FeatherDriftParticle(hitPos + Main.rand.NextVector2Circular(8f, 8f),
                        featherVel, IsBlack, 40, 0.5f));
            }

            // Music notes on flare impact
            SwanLakeVFXLibrary.SpawnMusicNotes(hitPos, 2, 15f, 0.5f, 0.8f, 22);

            // Empowered hit: extra sparkle burst
            if (IsEmpowered)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 burstVel = Main.rand.NextVector2CircularEdge(6f, 6f);
                    Color rainbow = BlackSwanUtils.GetRainbow((float)i / 8f);
                    Dust d = Dust.NewDustPerfect(hitPos, DustID.RainbowMk2, burstVel, 0, rainbow, 0.7f);
                    d.noGravity = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // Draw shader-driven trail
            DrawFlareTrail(sb);

            // Draw the flare core
            DrawFlareCore(sb);

            return false;
        }

        private void DrawFlareTrail(SpriteBatch sb)
        {
            if (_trailRenderer == null) return;

            // Build trail from old positions
            Vector2[] trailPositions = new Vector2[Projectile.oldPos.Length];
            for (int i = 0; i < trailPositions.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                {
                    trailPositions[i] = Projectile.Center;
                }
                else
                {
                    trailPositions[i] = Projectile.oldPos[i] + Projectile.Size / 2f;
                }
            }

            Color trailColor = IsBlack ? new Color(40, 40, 60) : new Color(200, 200, 220);
            Color glowColor = IsEmpowered ? BlackSwanUtils.GetRainbow() : (IsBlack ? new Color(60, 60, 90) : new Color(240, 240, 255));

            MiscShaderData shader = BlackSwanShaderLoader.GetFlareTrailShader();
            if (shader != null)
            {
                shader.UseColor(trailColor);
                shader.UseSecondaryColor(glowColor);
                try
                {
                    shader.Shader.Parameters["uTime"]?.SetValue(Main.GameUpdateCount * 0.03f);
                }
                catch { }
            }

            // Main trail
            var mainSettings = new BlackSwanTrailSettings(
                width: (float t) => MathHelper.Lerp(8f, 1f, t) * (IsEmpowered ? 1.3f : 1f),
                trailColor: (float t) => Color.Lerp(trailColor, Color.Transparent, t * t),
                shader: shader,
                smoothen: true
            );

            sb.End();
            _trailRenderer.RenderTrail(trailPositions, mainSettings, 30);

            // Glow overlay pass (additive)
            var glowSettings = new BlackSwanTrailSettings(
                width: (float t) => MathHelper.Lerp(12f, 2f, t) * (IsEmpowered ? 1.5f : 1f),
                trailColor: (float t) => new Color(glowColor.R, glowColor.G, glowColor.B, 0) * (1f - t) * 0.3f,
                shader: shader,
                smoothen: true
            );

            _trailRenderer.RenderTrail(trailPositions, glowSettings, 30);

            // Restore SpriteBatch
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawFlareCore(SpriteBatch sb)
        {
            Texture2D softRadial = null;
            Texture2D pointBloom = null;
            Texture2D starAccent = null;
            try
            {
                softRadial = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom",
                    AssetRequestMode.ImmediateLoad)?.Value;
                pointBloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom",
                    AssetRequestMode.ImmediateLoad)?.Value;
                starAccent = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft",
                    AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }

            if (softRadial == null && pointBloom == null) return;

            Vector2 screenPos = Projectile.Center - Main.screenPosition;

            float pulse = 0.8f + 0.2f * (float)Math.Sin(Projectile.timeLeft * 0.15f);
            float baseScale = IsEmpowered ? 0.5f : 0.35f;

            // Switch to additive
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Color outerColor = IsBlack ? new Color(40, 40, 60, 0) : new Color(220, 220, 240, 0);

            // Layer 1: Wide soft halo (SoftRadialBloom)
            if (softRadial != null)
            {
                Vector2 srOrigin = new Vector2(softRadial.Width / 2f, softRadial.Height / 2f);
                sb.Draw(softRadial, screenPos, null, outerColor * 0.35f * pulse, 0f, srOrigin, baseScale * 2.2f, SpriteEffects.None, 0f);

                // Layer 2: Mid polarity glow
                Color midColor = IsBlack ? new Color(60, 60, 85, 0) : new Color(200, 200, 230, 0);
                sb.Draw(softRadial, screenPos, null, midColor * 0.45f * pulse, 0f, srOrigin, baseScale * 1.2f, SpriteEffects.None, 0f);
            }

            // Layer 3: Intense core (PointBloom)
            if (pointBloom != null)
            {
                Vector2 pbOrigin = new Vector2(pointBloom.Width / 2f, pointBloom.Height / 2f);
                sb.Draw(pointBloom, screenPos, null, new Color(255, 255, 255, 0) * 0.7f * pulse, 0f, pbOrigin, baseScale * 0.6f, SpriteEffects.None, 0f);
            }

            // Layer 4: Rotating star accent
            if (starAccent != null)
            {
                Vector2 starOrigin = new Vector2(starAccent.Width / 2f, starAccent.Height / 2f);
                Color starCol = IsBlack ? new Color(100, 100, 140, 0) : new Color(240, 240, 255, 0);
                float starRot = Projectile.timeLeft * 0.12f;
                sb.Draw(starAccent, screenPos, null, starCol * 0.35f * pulse, starRot, starOrigin, baseScale * 0.4f, SpriteEffects.None, 0f);
            }

            // Empowered: rainbow ring
            if (IsEmpowered && softRadial != null)
            {
                Vector2 srOrigin = new Vector2(softRadial.Width / 2f, softRadial.Height / 2f);
                Color rainbow = BlackSwanUtils.GetRainbow(Projectile.timeLeft * 0.02f);
                sb.Draw(softRadial, screenPos, null, new Color(rainbow.R, rainbow.G, rainbow.B, 0) * 0.3f, 0f, srOrigin, baseScale * 3.5f, SpriteEffects.None, 0f);
            }

            // Restore
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override void OnKill(int timeLeft)
        {
            // Death VFX
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                BlackSwanParticleHandler.SpawnParticle(
                    new DualitySparkParticle(Projectile.Center, sparkVel, Main.rand.NextBool(), 15, 0.3f));
            }

            // Death music notes and feather scatter
            SwanLakeVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 12f, 0.5f, 0.7f, 20);
            SwanLakeVFXLibrary.SpawnFeatherDrift(Projectile.Center, 2, 15f, 0.2f);

            _trailRenderer?.Dispose();
        }
    }
}
