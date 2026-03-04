using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Utilities;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Projectiles
{
    /// <summary>
    /// Spectral wing bolt for Iridescent Wingspan (summoner staff secondary).
    /// 5-bolt fan pattern curves toward cursor. Empowered bolt is 3x larger, pen 5, noclip.
    /// Prismatic Convergence burst when 3+ bolts hit same target area.
    /// Foundation-pattern rendering: bloom trail, no primitives/custom particles.
    /// </summary>
    public class WingspanBoltProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        public ref float Timer => ref Projectile.ai[0];
        public ref float BoltIndex => ref Projectile.ai[1]; // 0-4 fan position; -1 = empowered
        public ref float CurveTarget => ref Projectile.localAI[0]; // 0 = normal (curve), 1 = empowered (straight)
        
        private const int TrailLength = 16;
        private Vector2[] oldPos = new Vector2[TrailLength];
        private float[] oldRot = new float[TrailLength];

        private Player Owner => Main.player[Projectile.owner];
        private bool IsEmpowered => BoltIndex == -1f;

        public override void SetStaticDefaults() { ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength; }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            Timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // --- Empowered bolt setup ---
            if (IsEmpowered && Timer == 1)
            {
                Projectile.penetrate = 5;
                Projectile.tileCollide = false;
                Projectile.scale = 1.5f;
                Projectile.width = 24;
                Projectile.height = 24;
            }

            // --- Cursor convergence (normal bolts curve toward mouse) ---
            if (!IsEmpowered && Timer > 10 && Timer < 120)
            {
                Vector2 cursorWorld = Main.MouseWorld;
                if (Projectile.owner == Main.myPlayer)
                {
                    Vector2 toCursor = (cursorWorld - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    float curveStrength = 0.035f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toCursor * Projectile.velocity.Length(), curveStrength);
                }
            }

            // --- Trail recording ---
            for (int i = TrailLength - 1; i > 0; i--)
            {
                oldPos[i] = oldPos[i - 1];
                oldRot[i] = oldRot[i - 1];
            }
            oldPos[0] = Projectile.Center;
            oldRot[0] = Projectile.rotation;

            // --- Spectral wing dust ---
            if (Timer % 2 == 0)
            {
                Color c = WingspanUtils.GetPrismaticEdge(Timer * 0.05f + BoltIndex * 0.3f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4),
                    DustID.WhiteTorch, -Projectile.velocity * 0.2f, 0, c, IsEmpowered ? 0.8f : 0.5f);
                d.noGravity = true;
            }

            // White ethereal sparkle
            if (Timer % 4 == 0)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.TintableDustLighted,
                    Main.rand.NextVector2Circular(2, 2), 0, WingspanUtils.EtherealWhite * 0.6f, 0.4f);
                d.noGravity = true;
            }

            // Empowered: additional gold feather dust
            if (IsEmpowered && Timer % 3 == 0)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(3, 3), 0, WingspanUtils.WingGold, 0.7f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.5f, 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SwansMark>(), 240);

            // Prismatic impact burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi / 8f * i;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color c = WingspanUtils.GetPrismaticEdge(i / 8f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch, vel, 0, c, 0.7f);
                d.noGravity = true;
            }

            // Convergence tracking: increment player's convergence counter
            try
            {
                var wp = Owner.GetModPlayer<WingspanPlayer>();
                if (wp != null)
                {
                    wp.ConvergenceCount++;
                    // Prismatic convergence burst at 3+ hits
                    if (wp.ConvergenceCount >= 3)
                    {
                        wp.ConvergenceCount = 0;
                        SpawnConvergenceBurst(target.Center);
                    }
                }
            }
            catch { }

            try { SwanLakeVFXLibrary.SpawnRainbowBurst(target.Center, 5, 3.5f); } catch { }
        }

        private void SpawnConvergenceBurst(Vector2 pos)
        {
            SoundEngine.PlaySound(SoundID.Item29, pos);

            // Prismatic burst ring
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi / 16f * i;
                Vector2 vel = angle.ToRotationVector2() * 5f;
                Color c = WingspanUtils.GetPrismaticEdge(i / 16f);
                Dust d = Dust.NewDustPerfect(pos, DustID.WhiteTorch, vel, 0, c, 1.0f);
                d.noGravity = true;
            }

            // Gold inner burst
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustPerfect(pos, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(6, 6), 0, WingspanUtils.WingGold, 1.2f);
                d.noGravity = true;
            }

            // Feather drift
            try { SwanLakeVFXLibrary.SpawnFeatherBurst(pos, 6, 0.3f); } catch { }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Color c = WingspanUtils.GetPrismaticEdge(i / 6f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(3, 3), 0, c, 0.5f);
                d.noGravity = true;
            }

            try { SwanLakeVFXLibrary.SpawnFeatherDrift(Projectile.Center, 2, 12f); } catch { }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 screenPos = Main.screenPosition;

            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Texture2D bloom = MagnumTextureRegistry.GetSoftGlow();
                Texture2D point = MagnumTextureRegistry.GetPointBloom();
                Texture2D star = MagnumTextureRegistry.GetStar4Soft();

                float boltScale = IsEmpowered ? 1.5f : 1f;

                // --- Prismatic bloom trail ---
                if (bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    for (int i = TrailLength - 1; i >= 1; i--)
                    {
                        if (oldPos[i] == Vector2.Zero) continue;
                        float progress = 1f - i / (float)TrailLength;
                        float trailAlpha = progress * 0.5f;
                        float trailScale = (0.2f + progress * 0.2f) * boltScale;

                        // Prismatic color cycling along trail
                        Color trailColor = WingspanUtils.GetPrismaticEdge(
                            i / (float)TrailLength + (float)Main.timeForVisualEffects * 0.008f + BoltIndex * 0.15f);
                        // Blend with ethereal white for brightness
                        trailColor = Color.Lerp(trailColor, WingspanUtils.EtherealWhite, 0.3f);

                        sb.Draw(bloom, oldPos[i] - screenPos, null, trailColor * trailAlpha,
                            oldRot[i], bOrigin, trailScale, SpriteEffects.None, 0f);
                    }
                }

                // --- Bolt core bloom ---
                Vector2 drawPos = Projectile.Center - screenPos;
                float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.09f + BoltIndex);

                // Outer spectral glow
                if (bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    Color outerColor = WingspanUtils.GetPrismaticEdge(Timer * 0.02f);
                    sb.Draw(bloom, drawPos, null, outerColor * 0.3f * pulse, 0f, bOrigin, 0.35f * boltScale * pulse, SpriteEffects.None, 0f);
                }

                // Mid glow (ethereal white)
                if (bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    sb.Draw(bloom, drawPos, null, WingspanUtils.EtherealWhite * 0.4f * pulse, 0f, bOrigin, 0.22f * boltScale * pulse, SpriteEffects.None, 0f);
                }

                // Hot white core
                if (point != null)
                {
                    Vector2 pOrigin = point.Size() * 0.5f;
                    sb.Draw(point, drawPos, null, Color.White * 0.8f, 0f, pOrigin, 0.15f * boltScale * pulse, SpriteEffects.None, 0f);
                }

                // Star sparkle accent (rotating)
                if (star != null)
                {
                    Vector2 sOrigin = star.Size() * 0.5f;
                    float starRot = (float)Main.timeForVisualEffects * 0.04f + BoltIndex;
                    Color starColor = WingspanUtils.GetPrismaticEdge(Timer * 0.015f + 0.5f);
                    sb.Draw(star, drawPos, null, starColor * 0.3f * pulse, starRot, sOrigin, 0.18f * boltScale, SpriteEffects.None, 0f);
                }

                // Empowered: wing-like prismatic accents on sides
                if (IsEmpowered && bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Vector2 wingOffset = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2 * side) * 12f;
                        Color wingColor = WingspanUtils.GetPrismaticEdge(Timer * 0.02f + side * 0.33f);
                        sb.Draw(bloom, drawPos + wingOffset, null, wingColor * 0.2f, 0f, bOrigin, 0.18f, SpriteEffects.None, 0f);
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

            // Theme accents (additive)
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            WingspanUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
