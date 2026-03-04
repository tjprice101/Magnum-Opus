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
    /// Fast white bullet projectile for The Swan's Lament (ranged gun).
    /// Grief-streak trail, feather shrapnel on hit, destruction halo on crit/special.
    /// Foundation-pattern rendering: SpriteBatch bloom trail, no primitives/custom particles.
    /// </summary>
    public class LamentBulletProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        public ref float Timer => ref Projectile.ai[0];
        public ref float SpawnMode => ref Projectile.ai[1]; // 0 = normal, 1 = empowered

        private const int TrailLength = 14;
        private Vector2[] oldPos = new Vector2[TrailLength];
        private float[] oldRot = new float[TrailLength];

        private Player Owner => Main.player[Projectile.owner];
        private bool IsEmpowered => SpawnMode == 1f;

        public override void SetStaticDefaults() { ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength; }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 2;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // --- Trail recording ---
            for (int i = TrailLength - 1; i > 0; i--)
            {
                oldPos[i] = oldPos[i - 1];
                oldRot[i] = oldRot[i - 1];
            }
            oldPos[0] = Projectile.Center;
            oldRot[0] = Projectile.rotation;

            // --- Grief smoke dust ---
            if (Timer % 3 == 0)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4),
                    DustID.Smoke, -Projectile.velocity * 0.15f, 80, LamentUtils.GriefGrey, 0.5f);
                d.noGravity = true;
            }

            // --- White core sparkle ---
            if (Timer % 5 == 0)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(1, 1), 0, LamentUtils.CatharsisWhite, 0.4f);
                d.noGravity = true;
            }

            // Empowered: gold accent dust
            if (IsEmpowered && Timer % 4 == 0)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(2, 2), 0, LamentUtils.RevelationGold, 0.6f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.6f, 0.6f, 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SwansMark>(), 240);

            // Feather shrapnel burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi / 6f * i + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color c = Color.Lerp(LamentUtils.CatharsisWhite, LamentUtils.GriefGrey, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch, vel, 0, c, 0.7f);
                d.noGravity = true;
            }

            // Gold flash on empowered
            if (IsEmpowered)
            {
                for (int i = 0; i < 4; i++)
                {
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                        Main.rand.NextVector2Circular(3, 3), 0, LamentUtils.RevelationGold, 0.9f);
                    d.noGravity = true;
                }
            }

            // VFXLibrary rainbow burst (safe)
            try { SwanLakeVFXLibrary.SpawnRainbowBurst(target.Center, 4, 3f); } catch { }
        }

        public override void OnKill(int timeLeft)
        {
            // Small grief puff
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke,
                    Main.rand.NextVector2Circular(3, 3), 60, LamentUtils.GriefGrey, 0.6f);
                d.noGravity = false;
            }

            // Mourning feather drift
            try { SwanLakeVFXLibrary.SpawnFeatherDrift(Projectile.Center, 2, 15f); } catch { }

            // Empowered: spawn Destruction Halo
            if (IsEmpowered && Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                    Vector2.Zero, ModContent.ProjectileType<DestructionHaloProj>(),
                    Projectile.damage / 2, 0f, Projectile.owner);
            }
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
                Texture2D streak = MagnumTextureRegistry.GetBeamStreak();

                // --- Grief streak trail (beam-like) ---
                if (streak != null)
                {
                    Vector2 strkOrigin = new Vector2(streak.Width * 0.5f, streak.Height * 0.5f);
                    for (int i = TrailLength - 1; i >= 1; i--)
                    {
                        if (oldPos[i] == Vector2.Zero) continue;
                        float progress = 1f - i / (float)TrailLength;
                        float alpha = progress * 0.5f;
                        float scaleY = 0.08f + progress * 0.06f;
                        float scaleX = 0.4f;

                        // Gradient from black → grey → white along trail
                        Color trailColor = Color.Lerp(LamentUtils.MourningBlack, LamentUtils.CatharsisWhite, progress);
                        // Subtle gold accent on empowered
                        if (IsEmpowered)
                            trailColor = Color.Lerp(trailColor, LamentUtils.RevelationGold, 0.15f);

                        sb.Draw(streak, oldPos[i] - screenPos, null, trailColor * alpha,
                            oldRot[i], strkOrigin, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    }
                }

                // --- Soft bloom trail for glow ---
                if (bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    for (int i = TrailLength - 1; i >= 1; i--)
                    {
                        if (oldPos[i] == Vector2.Zero) continue;
                        float progress = 1f - i / (float)TrailLength;
                        Color glowColor = Color.Lerp(LamentUtils.GriefGrey, LamentUtils.CatharsisWhite, progress);
                        sb.Draw(bloom, oldPos[i] - screenPos, null, glowColor * (progress * 0.2f),
                            0f, bOrigin, 0.15f + progress * 0.1f, SpriteEffects.None, 0f);
                    }
                }

                // --- Bullet core bloom ---
                Vector2 drawPos = Projectile.Center - screenPos;
                float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.1f);

                // Outer grey glow
                if (bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    sb.Draw(bloom, drawPos, null, LamentUtils.GriefGrey * 0.3f * pulse, 0f, bOrigin, 0.3f * pulse, SpriteEffects.None, 0f);
                }

                // White core
                if (point != null)
                {
                    Vector2 pOrigin = point.Size() * 0.5f;
                    float coreScale = IsEmpowered ? 0.2f : 0.15f;
                    sb.Draw(point, drawPos, null, Color.White * 0.85f, 0f, pOrigin, coreScale * pulse, SpriteEffects.None, 0f);
                }

                // Gold accent ring for empowered
                if (IsEmpowered && bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = MathHelper.TwoPi / 4f * i + (float)Main.timeForVisualEffects * 0.05f;
                        Vector2 offset = angle.ToRotationVector2() * 5f;
                        sb.Draw(bloom, drawPos + offset, null, LamentUtils.RevelationGold * 0.25f, 0f, bOrigin, 0.1f, SpriteEffects.None, 0f);
                    }
                }

                // --- Draw bullet sprite ---
                Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
                if (tex != null)
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    Vector2 texOrigin = tex.Size() * 0.5f;
                    sb.Draw(tex, drawPos, null, Projectile.GetAlpha(lightColor), Projectile.rotation + MathHelper.PiOver2,
                        texOrigin, Projectile.scale, SpriteEffects.None, 0f);

                    // Don't need to switch back; finally block handles it
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
            LamentUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
