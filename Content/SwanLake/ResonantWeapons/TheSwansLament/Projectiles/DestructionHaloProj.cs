using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Utilities;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Shaders;
using Terraria.Graphics.Shaders;
using ReLogic.Content;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Projectiles
{
    /// <summary>
    /// Expanding halo ring projectile for The Swan's Lament.
    /// Ring-shaped collision (edge only), EaseOutQuart expansion 20px → 180px over 2s.
    /// Applies MournfulGaze debuff. Foundation-pattern rendering.
    /// </summary>
    public class DestructionHaloProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        public ref float Timer => ref Projectile.ai[0];

        private const float StartRadius = 20f;
        private const float MaxRadius = 180f;
        private const int Duration = 120; // 2 seconds
        private const float RingThickness = 28f; // How thick the damage ring is

        private float Expansion => EaseOutQuart(MathHelper.Clamp(Timer / Duration, 0f, 1f));
        private float CurrentRadius => MathHelper.Lerp(StartRadius, MaxRadius, Expansion);

        private static float EaseOutQuart(float t) => 1f - MathF.Pow(1f - t, 4f);

        public override void SetDefaults()
        {
            Projectile.width = 360;
            Projectile.height = 360;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Hit once per NPC
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Timer++;

            // Slow velocity decay (if spawned with velocity)
            Projectile.velocity *= 0.96f;

            // Resize hitbox to encompass the ring
            float radius = CurrentRadius;
            int newSize = (int)(radius * 2 + RingThickness);
            if (Math.Abs(Projectile.width - newSize) > 4)
            {
                Projectile.position += new Vector2((Projectile.width - newSize) / 2f, (Projectile.height - newSize) / 2f);
                Projectile.width = newSize;
                Projectile.height = newSize;
            }

            // Ring edge dust
            if (Timer % 2 == 0)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 ringPos = Projectile.Center + angle.ToRotationVector2() * radius;
                Color c = Color.Lerp(LamentUtils.CatharsisWhite, LamentUtils.GriefGrey, Main.rand.NextFloat(0.3f));
                Dust d = Dust.NewDustPerfect(ringPos, DustID.WhiteTorch,
                    angle.ToRotationVector2() * 1.5f, 0, c, 0.6f);
                d.noGravity = true;
            }

            // Gold accent particles at ring front
            if (Timer % 6 == 0)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 ringPos = Projectile.Center + angle.ToRotationVector2() * radius;
                Dust d = Dust.NewDustPerfect(ringPos, DustID.WhiteTorch,
                    angle.ToRotationVector2() * 0.5f, 0, LamentUtils.RevelationWhite, 0.5f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.4f, 0.4f, 0.5f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Ring-shaped collision: only hits at the edge of the ring, not inside
            float dist = Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2());
            float targetRadius = Math.Max(targetHitbox.Width, targetHitbox.Height) / 2f;
            float radius = CurrentRadius;

            float innerEdge = radius - RingThickness / 2f;
            float outerEdge = radius + RingThickness / 2f;

            return dist - targetRadius < outerEdge && dist + targetRadius > innerEdge;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MournfulGaze>(), 300);
            target.AddBuff(ModContent.BuffType<SwansMark>(), 180);

            // Impact flash at hit point
            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(3, 3), 0, LamentUtils.CatharsisWhite, 0.8f);
                d.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Final flash burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi / 16f * i;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                Dust d = Dust.NewDustPerfect(Projectile.Center + angle.ToRotationVector2() * CurrentRadius,
                    DustID.WhiteTorch, vel, 0, LamentUtils.CatharsisWhite, 0.5f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float radius = CurrentRadius;
            float lifeProgress = Timer / Duration;

            // Fade: full near start, fading out in last 30%
            float opacity = lifeProgress > 0.7f ? MathHelper.Lerp(1f, 0f, (lifeProgress - 0.7f) / 0.3f) : 1f;

            try
            {
                sb.End();

                Texture2D bloom = MagnumTextureRegistry.GetSoftGlow();
                Texture2D point = MagnumTextureRegistry.GetPointBloom();
                Texture2D halo = MagnumTextureRegistry.GetHaloRing();
                Texture2D star = MagnumTextureRegistry.GetStar4Soft();

                // ============ NOISE-SCROLLED HALO ZONE ============
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                float time = (float)Main.timeForVisualEffects * 0.015f;

                // Visual radius scaled down from gameplay radius
                float visualRadius = radius * 0.35f * MathHelper.Lerp(0.3f, 1f, lifeProgress);

                // Primary sparkle impact zone — grief grey to catharsis white
                Color outerGrief = Color.Lerp(LamentUtils.GriefGrey, LamentUtils.CatharsisWhite, 0.2f);
                Color innerCatharsis = Color.Lerp(LamentUtils.CatharsisWhite, Color.White, 0.3f);
                SwanLakeVFXLibrary.DrawPrismaticSparkleImpact(sb, Projectile.Center, visualRadius, time, opacity * 0.65f, 10);

                // Secondary smaller zone for inner intensity
                Color innerGold = Color.Lerp(LamentUtils.RevelationWhite, LamentUtils.CatharsisWhite, 0.5f);
                SwanLakeVFXLibrary.DrawPrismaticSparkleImpact(sb, Projectile.Center, visualRadius * 0.55f, time * 1.3f, opacity * 0.4f, 6);

                // Halo ring texture overlay at expansion edge
                if (halo != null)
                {
                    Vector2 hOrigin = halo.Size() * 0.5f;
                    float haloScale = (radius * 2f) / halo.Width;
                    float spin = (float)Main.timeForVisualEffects * 0.01f;
                    sb.Draw(halo, drawPos, null, LamentUtils.CatharsisWhite * 0.35f * opacity, spin, hOrigin, haloScale, SpriteEffects.None, 0f);
                }

                // Star sparkle at center
                if (star != null)
                {
                    Vector2 sOrigin = star.Size() * 0.5f;
                    float starRot = (float)Main.timeForVisualEffects * 0.02f;
                    sb.Draw(star, drawPos, null, LamentUtils.RevelationWhite * 0.18f * opacity, starRot, sOrigin, 0.18f, SpriteEffects.None, 0f);
                }

                // Point accents at cardinal positions on the ring edge
                if (point != null)
                {
                    Vector2 pOrigin = point.Size() * 0.5f;
                    float rotOffset = (float)Main.timeForVisualEffects * 0.025f;
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi / 6f * i + rotOffset;
                        Vector2 accentPos = drawPos + angle.ToRotationVector2() * radius;
                        sb.Draw(point, accentPos, null, Color.White * 0.45f * opacity, 0f, pOrigin, 0.1f, SpriteEffects.None, 0f);
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
