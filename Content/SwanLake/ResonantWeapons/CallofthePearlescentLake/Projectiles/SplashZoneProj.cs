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
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Texture2D bloom = MagnumTextureRegistry.GetSoftGlow();
                Texture2D radial = MagnumTextureRegistry.GetRadialBloom();
                Texture2D point = MagnumTextureRegistry.GetPointBloom();

                // --- Base zone glow (wide radial) ---
                if (radial != null)
                {
                    Vector2 rOrigin = radial.Size() * 0.5f;
                    float baseScale = radius / (radial.Width * 0.5f);
                    Color baseColor = Color.Lerp(PearlescentUtils.DeepLake, PearlescentUtils.MistBlue, 0.5f);
                    sb.Draw(radial, drawPos, null, baseColor * 0.15f * opacity, 0f, rOrigin, baseScale, SpriteEffects.None, 0f);
                }

                // --- Inner pearl shimmer ---
                if (bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    float innerScale = radius * 0.7f / (bloom.Width * 0.5f);
                    float shimmer = 0.85f + 0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.06f);
                    sb.Draw(bloom, drawPos, null, PearlescentUtils.PearlWhite * 0.12f * opacity * shimmer,
                        0f, bOrigin, innerScale, SpriteEffects.None, 0f);
                }

                // --- Edge ring (pixel dots around circumference) ---
                if (point != null)
                {
                    Vector2 pOrigin = point.Size() * 0.5f;
                    int ringDots = 24;
                    for (int i = 0; i < ringDots; i++)
                    {
                        float angle = MathHelper.TwoPi / ringDots * i + (float)Main.timeForVisualEffects * 0.015f;
                        Vector2 dotPos = drawPos + angle.ToRotationVector2() * radius;
                        Color dotColor = Color.Lerp(PearlescentUtils.PearlWhite, PearlescentUtils.GetRainbow(i / (float)ringDots), 0.35f);
                        sb.Draw(point, dotPos, null, dotColor * 0.4f * opacity, 0f, pOrigin, 0.12f, SpriteEffects.None, 0f);
                    }
                }

                // --- Slowly rotating rainbow accent ring (larger, softer) ---
                if (bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i - (float)Main.timeForVisualEffects * 0.01f;
                        Vector2 accentPos = drawPos + angle.ToRotationVector2() * (radius * 0.85f);
                        Color rc = PearlescentUtils.GetRainbow(i / 8f + (float)Main.timeForVisualEffects * 0.005f);
                        sb.Draw(bloom, accentPos, null, rc * 0.08f * opacity, 0f, bOrigin, 0.2f, SpriteEffects.None, 0f);
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
