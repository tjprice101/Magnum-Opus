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
                    angle.ToRotationVector2() * 0.5f, 0, LamentUtils.RevelationGold, 0.5f);
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
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Texture2D radial = MagnumTextureRegistry.GetRadialBloom();
                Texture2D bloom = MagnumTextureRegistry.GetSoftGlow();
                Texture2D point = MagnumTextureRegistry.GetPointBloom();
                Texture2D halo = MagnumTextureRegistry.GetHaloRing();

                // --- Soft radial bloom backdrop (the "halo glow") ---
                if (radial != null)
                {
                    Vector2 rOrigin = radial.Size() * 0.5f;
                    float backdropScale = (radius + 30f) / (radial.Width * 0.5f);
                    Color backdropColor = Color.Lerp(LamentUtils.GriefGrey, LamentUtils.CatharsisWhite, 0.3f);
                    sb.Draw(radial, drawPos, null, backdropColor * 0.12f * opacity, 0f, rOrigin, backdropScale, SpriteEffects.None, 0f);
                }

                // --- Halo ring texture if available ---
                if (halo != null)
                {
                    Vector2 hOrigin = halo.Size() * 0.5f;
                    float haloScale = (radius * 2f) / halo.Width;
                    float spin = (float)Main.timeForVisualEffects * 0.01f;
                    sb.Draw(halo, drawPos, null, LamentUtils.CatharsisWhite * 0.5f * opacity, spin, hOrigin, haloScale, SpriteEffects.None, 0f);
                    // Gold accent layer
                    sb.Draw(halo, drawPos, null, LamentUtils.RevelationGold * 0.15f * opacity, -spin * 0.7f, hOrigin, haloScale * 1.05f, SpriteEffects.None, 0f);
                }
                else if (bloom != null)
                {
                    // Fallback: construct ring from bloom dots
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    int ringSegments = 32;
                    for (int i = 0; i < ringSegments; i++)
                    {
                        float angle = MathHelper.TwoPi / ringSegments * i;
                        Vector2 ringPos = drawPos + angle.ToRotationVector2() * radius;
                        Color c = Color.Lerp(LamentUtils.CatharsisWhite, LamentUtils.GriefGrey, 0.2f);
                        // Outer ring
                        sb.Draw(bloom, ringPos, null, c * 0.35f * opacity, 0f, bOrigin, 0.18f, SpriteEffects.None, 0f);
                    }
                    // Inner ring (slightly smaller, darker) to create ring effect
                    float innerRadius = radius * 0.85f;
                    for (int i = 0; i < ringSegments; i++)
                    {
                        float angle = MathHelper.TwoPi / ringSegments * i + 0.05f;
                        Vector2 ringPos = drawPos + angle.ToRotationVector2() * innerRadius;
                        sb.Draw(bloom, ringPos, null, LamentUtils.GriefGrey * 0.2f * opacity, 0f, bOrigin, 0.12f, SpriteEffects.None, 0f);
                    }
                }

                // --- Bright point accents at cardinal positions (rotating) ---
                if (point != null)
                {
                    Vector2 pOrigin = point.Size() * 0.5f;
                    float rotOffset = (float)Main.timeForVisualEffects * 0.025f;
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = MathHelper.PiOver2 * i + rotOffset;
                        Vector2 accentPos = drawPos + angle.ToRotationVector2() * radius;
                        sb.Draw(point, accentPos, null, Color.White * 0.6f * opacity, 0f, pOrigin, 0.15f, SpriteEffects.None, 0f);
                    }
                }

                // --- Gold shimmer trailing the expansion ---
                if (bloom != null && lifeProgress < 0.6f)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi / 6f * i + Timer * 0.08f;
                        Vector2 shimmerPos = drawPos + angle.ToRotationVector2() * (radius * 0.95f);
                        sb.Draw(bloom, shimmerPos, null, LamentUtils.RevelationGold * 0.2f * opacity, 0f, bOrigin, 0.1f, SpriteEffects.None, 0f);
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
