using System;
using MagnumOpus.Common;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles
{
    /// <summary>
    /// CelestialValorNoiseZone -- Persistent AoE damage zone spawned at flying blade impact point.
    /// Dual-layer additive bloom with slow swirl, scarlet/gold/white fire coloring.
    /// Periodically spawns CelestialValorZoneOrb homing projectiles from edge positions.
    /// 2-second duration with fade-in and fade-out.
    ///
    /// Rendering approach: Multi-layer additive SoftGlow/PointBloom circles with slow rotation
    /// to create a swirling fire zone effect without requiring a custom shader.
    /// </summary>
    public class CelestialValorNoiseZone : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int MaxLifetime = 120;
        private const int FadeInFrames = 10;
        private const int FadeOutFrames = 20;
        private const float ZoneRadius = 100f;

        private int timer;
        private float seed;

        // Cached textures
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _pointBloom;

        // Eroica fire palette
        private static readonly Color ScarletFire = new Color(255, 60, 40);
        private static readonly Color GoldFire = new Color(255, 200, 80);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 500;
        }

        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = MaxLifetime;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (timer == 0)
                seed = Main.rand.NextFloat(100f);

            timer++;
            Projectile.velocity = Vector2.Zero;

            float alpha = GetAlphaMultiplier();

            // Lighting (scarlet-gold)
            Lighting.AddLight(Projectile.Center, 0.8f * alpha, 0.3f * alpha, 0.2f * alpha);

            // Scale pulsing
            Projectile.scale = 1f + 0.1f * MathF.Sin(Main.GlobalTimeWrappedHourly * 4f);

            // Every 20 ticks: spawn 2-3 CelestialValorZoneOrb projectiles from random edge positions
            if (timer % 20 == 0 && Projectile.owner == Main.myPlayer)
            {
                int orbCount = Main.rand.Next(2, 4);
                for (int i = 0; i < orbCount; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * ZoneRadius;
                    Vector2 orbVel = Main.rand.NextVector2Circular(2f, 2f);

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromAI(),
                        spawnPos, orbVel,
                        ModContent.ProjectileType<CelestialValorZoneOrb>(),
                        Projectile.damage, 0f, Projectile.owner);
                }
            }

            // Ambient fire dust inside zone
            if (!Main.dedServ && Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = Main.rand.NextFloat(ZoneRadius * 0.8f);
                Vector2 dustPos = Projectile.Center + angle.ToRotationVector2() * radius;
                Vector2 dustVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.5f, 2f));

                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, dustVel, 0,
                    default, Main.rand.NextFloat(0.8f, 1.4f));
                d.noGravity = true;
            }
        }

        private float GetAlphaMultiplier()
        {
            float fadeIn = MathHelper.Clamp(timer / (float)FadeInFrames, 0f, 1f);
            float fadeOut = Projectile.timeLeft < FadeOutFrames
                ? Projectile.timeLeft / (float)FadeOutFrames
                : 1f;
            return fadeIn * fadeOut;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 closestPoint = new Vector2(
                MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
                MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
            float dist = Vector2.Distance(Projectile.Center, closestPoint);
            return dist <= ZoneRadius;
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            SpriteBatch sb = Main.spriteBatch;
            try
            {
                LoadTextures();

                Texture2D softGlow = _softGlow?.Value;
                Texture2D pointBloom = _pointBloom?.Value;
                if (softGlow == null || pointBloom == null) return false;

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Vector2 glowOrigin = softGlow.Size() / 2f;
                Vector2 bloomOrigin = pointBloom.Size() / 2f;
                float alpha = GetAlphaMultiplier();
                float scale = Projectile.scale;
                float rotation = Main.GlobalTimeWrappedHourly * 1.5f;

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                // Layer 1: Large outer scarlet glow
                sb.Draw(softGlow, drawPos, null,
                    (ScarletFire with { A = 0 }) * (0.3f * alpha),
                    rotation, glowOrigin, 0.35f * scale, SpriteEffects.None, 0f);

                // Layer 2: Counter-rotated inner gold glow
                sb.Draw(softGlow, drawPos, null,
                    (GoldFire with { A = 0 }) * (0.5f * alpha),
                    -rotation * 0.7f, glowOrigin, 0.22f * scale, SpriteEffects.None, 0f);

                // Layer 3: Hot white core
                sb.Draw(pointBloom, drawPos, null,
                    (Color.White with { A = 0 }) * (0.6f * alpha),
                    0f, bloomOrigin, 0.08f * scale, SpriteEffects.None, 0f);

                // Layer 4: Edge sparkles orbiting the zone
                DrawEdgeSparkles(sb, pointBloom, bloomOrigin, drawPos, alpha);
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

        private void DrawEdgeSparkles(SpriteBatch sb, Texture2D pointBloom, Vector2 bloomOrigin,
            Vector2 drawPos, float alpha)
        {
            float time = (float)Main.timeForVisualEffects;
            int sparkleCount = 6;

            for (int i = 0; i < sparkleCount; i++)
            {
                float baseAngle = (i / (float)sparkleCount) * MathHelper.TwoPi;
                float animAngle = baseAngle + time * 0.01f + seed;
                float radiusOffset = 0.8f + 0.15f * MathF.Sin(time * 0.04f + i * 1.8f);

                Vector2 sparkleOffset = animAngle.ToRotationVector2() * (ZoneRadius * radiusOffset * 0.6f);
                float sparkleAlpha = 0.3f + 0.25f * MathF.Sin(time * 0.07f + i * 2.3f);
                float sparkleScale = 0.06f + 0.03f * MathF.Sin(time * 0.09f + i * 1.5f);

                Color sparkleColor = i % 2 == 0
                    ? (GoldFire with { A = 0 })
                    : (ScarletFire with { A = 0 });

                sb.Draw(pointBloom, drawPos + sparkleOffset, null,
                    sparkleColor * (sparkleAlpha * alpha),
                    0f, bloomOrigin, sparkleScale, SpriteEffects.None, 0f);
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;

        private static void LoadTextures()
        {
            const string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
            _softGlow ??= ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>(Bloom + "PointBloom", AssetRequestMode.ImmediateLoad);
        }
    }
}
