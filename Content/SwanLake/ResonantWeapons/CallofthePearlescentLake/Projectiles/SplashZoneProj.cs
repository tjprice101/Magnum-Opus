using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Utilities;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Shaders;
using Terraria.Graphics.Shaders;
using ReLogic.Content;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Projectiles
{
    /// <summary>
    /// Persistent AoE damage field spawned by PearlescentRocketProj on-kill.
    /// 5s duration, 25% slow debuff, expanding shimmer zone.
    /// Foundation-pattern rendering: bloom layers + pixel ring edge, no primitives/custom particles.
    /// </summary>
    public class SplashZoneProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        public ref float Timer => ref Projectile.ai[0];

        private const float MaxRadius = 120f;
        private const int Duration = 300; // 5 seconds
        private float CurrentRadius => MathHelper.Lerp(20f, MaxRadius, MathHelper.Clamp(Timer / 30f, 0f, 1f));
        private float LifeProgress => Timer / Duration;

        public override void SetDefaults()
        {
            Projectile.width = 240;
            Projectile.height = 240;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Timer++;

            // Fade in
            if (Timer < 15)
                Projectile.alpha = (int)MathHelper.Lerp(255, 0, Timer / 15f);
            // Fade out in last second
            else if (Projectile.timeLeft < 60)
                Projectile.alpha = (int)MathHelper.Lerp(0, 255, 1f - Projectile.timeLeft / 60f);
            else
                Projectile.alpha = 0;

            // Resize hitbox to match visual radius
            float radius = CurrentRadius;
            int newSize = (int)(radius * 2);
            if (Projectile.width != newSize)
            {
                Projectile.position += new Vector2((Projectile.width - newSize) / 2f, (Projectile.height - newSize) / 2f);
                Projectile.width = newSize;
                Projectile.height = newSize;
            }

            // Edge dust ring
            if (Timer % 3 == 0)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 edgePos = Projectile.Center + angle.ToRotationVector2() * radius;
                Color c = Color.Lerp(PearlescentUtils.PearlWhite, PearlescentUtils.GetRainbow(angle / MathHelper.TwoPi), 0.3f);
                Dust d = Dust.NewDustPerfect(edgePos, DustID.WhiteTorch,
                    angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 0.8f, 0, c, 0.5f);
                d.noGravity = true;
            }

            // Interior shimmer
            if (Timer % 5 == 0)
            {
                Vector2 randomPos = Projectile.Center + Main.rand.NextVector2Circular(radius * 0.7f, radius * 0.7f);
                Dust d = Dust.NewDustPerfect(randomPos, DustID.TintableDustLighted,
                    Vector2.UnitY * -0.5f, 0, PearlescentUtils.LakeSilver * 0.6f, 0.4f);
                d.noGravity = true;
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, 0.5f, 0.5f, 0.6f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Circular collision
            float dist = Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2());
            return dist < CurrentRadius + Math.Max(targetHitbox.Width, targetHitbox.Height) / 2f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Slow debuff
            target.AddBuff(BuffID.Slow, 60);
            target.AddBuff(ModContent.BuffType<SwansMark>(), 180);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float opacity = 1f - Projectile.alpha / 255f;
            float radius = CurrentRadius;

            try
            {
                sb.End();

                Texture2D bloom = MagnumTextureRegistry.GetSoftGlow();
                Texture2D point = MagnumTextureRegistry.GetPointBloom();
                Texture2D star = MagnumTextureRegistry.GetStar4Soft();

                // ============ NOISE UV-SCROLLED ZONE (replaces shader + flat bloom layers) ============
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                float time = (float)Main.timeForVisualEffects;

                // Primary pearlescent sparkle zone (lake-themed)
                Color lakeOuter = Color.Lerp(PearlescentUtils.DeepLake, PearlescentUtils.MistBlue, 0.4f);
                Color lakeInner = PearlescentUtils.PearlWhite;
                SwanLakeVFXLibrary.DrawPrismaticSparkleImpact(sb, Projectile.Center, radius,
                    time, opacity * 0.7f, 10);

                // Secondary rainbow-tinted sparkle layer for iridescent shimmer
                SwanLakeVFXLibrary.DrawPrismaticSparkleImpact(sb, Projectile.Center, radius * 0.6f,
                    time + 50f, opacity * 0.25f, 6);

                float shimmer = 0.85f + 0.15f * MathF.Sin(time * 0.06f);

                // Center bright point
                if (point != null)
                {
                    Vector2 pOrigin = point.Size() * 0.5f;
                    sb.Draw(point, drawPos, null, Color.White * 0.35f * opacity * shimmer,
                        0f, pOrigin, 0.12f * shimmer, SpriteEffects.None, 0f);
                }

                // Star sparkle at center (rotating)
                if (star != null)
                {
                    Vector2 sOrigin = star.Size() * 0.5f;
                    float starRot = time * 0.02f;
                    Color starColor = PearlescentUtils.GetRainbow(time * 0.004f);
                    sb.Draw(star, drawPos, null, starColor * 0.15f * opacity, starRot, sOrigin, 0.2f, SpriteEffects.None, 0f);
                }

                // Edge ring dots (circumference definition)
                if (point != null)
                {
                    Vector2 pOrigin2 = point.Size() * 0.5f;
                    int ringDots = 32;
                    for (int i = 0; i < ringDots; i++)
                    {
                        float angle = MathHelper.TwoPi / ringDots * i + time * 0.015f;
                        Vector2 dotPos = drawPos + angle.ToRotationVector2() * radius;
                        Color dotColor = Color.Lerp(PearlescentUtils.PearlWhite,
                            PearlescentUtils.GetRainbow(i / (float)ringDots + time * 0.003f), 0.4f);
                        sb.Draw(point, dotPos, null, dotColor * 0.35f * opacity, 0f, pOrigin2, 0.08f, SpriteEffects.None, 0f);
                    }
                }

                // Rainbow accent dots (slowly rotating inner ring)
                if (bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i - time * 0.012f;
                        Vector2 accentPos = drawPos + angle.ToRotationVector2() * (radius * 0.75f);
                        Color rc = PearlescentUtils.GetRainbow(i / 8f + time * 0.005f);
                        sb.Draw(bloom, accentPos, null, rc * 0.1f * opacity, 0f, bOrigin, 0.12f, SpriteEffects.None, 0f);
                    }
                }
            }
            catch { }
            finally
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}
